/* NC_Tool core: DXF (tấm kim loại trải phôi) -> NC CamMagic LA (Mitsubishi)
 * Quy tắc màu (Layer 0):
 *   by layer : LINE/ARC/CIRCLE -> biên ngoài, lỗ, (đường hở = khe cắt)  M98P9010
 *              POINT           -> lỗ trắng                               M98P9016
 *   xanh lá (ACI 3): LINE/ARC  -> khe cắt (đường hở, piercing ngoài chi tiết) M98P9010
 *                    POINT     -> piercing xuyên (chỉ đục 1 điểm)        M98P9010
 *   đỏ (ACI 1): LINE/ARC/CIRCLE -> kegaki (vạch dấu)                    M98P9017
 *   nét đứt (DASHED) -> đường uốn, bỏ qua
 */
(function (root) {
  'use strict';

  class NCError extends Error {}
  const TAU = 2 * Math.PI;
  const mod = (a, m) => ((a % m) + m) % m;
  const key = p => Math.round(p[0] * 1000) + ',' + Math.round(p[1] * 1000);
  const dist = (a, b) => Math.hypot(a[0] - b[0], a[1] - b[1]);

  function fmt(v) {
    v = Math.sign(v) * Math.round(Math.abs(v) * 1000) / 1000;
    if (Math.abs(v) < 1e-9) v = 0;
    return v.toFixed(3).replace(/0+$/, '');          // 21.000 -> "21." ; 46.800 -> "46.8"
  }

  // ---------- Bảng thông số (Quy_Tac_Laser.md mục 2): [piercing ngoài, joint ngoài, piercing lỗ] ----------
  const TABLE = {
    SS:  [[3.2, [3, 0.8, 6]], [5, [6, 1, 6]], [9, [6, 1, 6]], [16, [10, 1, 10]]],
    SUS: [[8, [6, 0.7, 6]], [16, [10, 1, 10]]],
    AL:  [[8, [6, 0.7, 6]], [18, [10, 1, 10]]],
    CU:  [[99, [6, 0.7, 6]]]
  };
  function params(group, t) {
    const rows = TABLE[String(group).toUpperCase()];
    if (!rows) throw new NCError('Nhóm vật liệu không hợp lệ: ' + group);
    if (!(t > 0)) throw new NCError('Độ dày không hợp lệ');
    for (const [tmax, p] of rows) if (t <= tmax + 1e-9) return { pierceOut: p[0], jointOut: p[1], pierceHole: p[2] };
    throw new NCError('Độ dày ' + t + ' của nhóm ' + group + ' dùng piercing cong (≥19t): chưa hỗ trợ');
  }

  // ---------- Đọc DXF ----------
  function colorOf(e) {
    const c = e.g.find(x => x[0] === '62');
    const n = c ? parseInt(c[1], 10) : 256;
    if (n === 256 || n === 0 || n === 7) return 'L';   // by layer / by block / trắng
    if (n === 1) return 'red';
    if (n === 3) return 'green';
    return 'other';
  }

  function parseDXF(input) {
    let text = input;
    if (typeof input !== 'string') text = new TextDecoder('utf-8').decode(new Uint8Array(input));
    if (text.startsWith('AutoCAD Binary DXF')) throw new NCError('DXF dạng binary: hãy lưu lại dạng ASCII');
    const lines = text.split(/\r?\n/);
    const pairs = [];
    for (let i = 0; i + 1 < lines.length; i += 2) pairs.push([lines[i].trim(), lines[i + 1].trim()]);
    const ents = []; let cur = null, inEnt = false;
    for (let i = 0; i < pairs.length; i++) {
      const [c, v] = pairs[i];
      if (!inEnt) {
        if (c === '2' && v === 'ENTITIES' && i > 0 && pairs[i - 1][0] === '0' && pairs[i - 1][1] === 'SECTION') inEnt = true;
        continue;
      }
      if (c === '0') {
        if (cur) ents.push(cur);
        cur = null;
        if (v === 'ENDSEC') break;
        cur = { type: v, g: [] };
      } else if (cur) cur.g.push([c, v]);
    }
    if (!inEnt) throw new NCError('Không tìm thấy section ENTITIES trong DXF');

    const get = (e, c) => { const x = e.g.find(y => y[0] === c); return x ? x[1] : undefined; };
    const num = (e, c) => parseFloat(get(e, c));
    const P = { segs: [], circles: [], points: [], warnings: [] };
    const ignored = {};
    const ign = k => { ignored[k] = (ignored[k] || 0) + 1; };

    const pushArcBulge = (p, q, b, col) => {
      if (Math.abs(b) < 1e-9) { P.segs.push({ t: 'L', p, q, col, ccw: true }); return; }
      const d = dist(p, q); if (d < 1e-9) return;
      const th = 4 * Math.atan(b);
      const r = d / (2 * Math.sin(Math.abs(th) / 2));
      const ux = (q[0] - p[0]) / d, uy = (q[1] - p[1]) / d;
      const off = (d / 2) / Math.tan(th / 2);
      const c = [(p[0] + q[0]) / 2 - uy * off, (p[1] + q[1]) / 2 + ux * off];
      if (b > 0) P.segs.push({ t: 'A', c, r, p, q, sweep: Math.abs(th), col, ccw: true });
      else P.segs.push({ t: 'A', c, r, p: q, q: p, sweep: Math.abs(th), col, ccw: true });
    };

    for (const e of ents) {
      const lt = (get(e, '6') || '').toUpperCase();
      const dashed = lt && !/^(CONTINUOUS|BYLAYER|BYBLOCK)$/.test(lt);
      const col = colorOf(e);
      switch (e.type) {
        case 'LINE': case 'ARC': case 'CIRCLE': case 'LWPOLYLINE':
          if (dashed) { ign('đường nét đứt (đường uốn)'); break; }
          if (col === 'other') { ign('đối tượng màu khác (không phải by layer/xanh/đỏ)'); break; }
          if (e.type === 'LINE') {
            P.segs.push({ t: 'L', p: [num(e, '10'), num(e, '20')], q: [num(e, '11'), num(e, '21')], col, ccw: true });
          } else if (e.type === 'ARC') {
            const c = [num(e, '10'), num(e, '20')], r = num(e, '40');
            const a0 = num(e, '50') * Math.PI / 180, a1 = num(e, '51') * Math.PI / 180;
            let sw = mod(a1 - a0, TAU); if (sw < 1e-9) sw = TAU;
            P.segs.push({ t: 'A', c, r, p: [c[0] + r * Math.cos(a0), c[1] + r * Math.sin(a0)],
                          q: [c[0] + r * Math.cos(a1), c[1] + r * Math.sin(a1)], sweep: sw, col, ccw: true });
          } else if (e.type === 'CIRCLE') {
            P.circles.push({ cx: num(e, '10'), cy: num(e, '20'), r: num(e, '40'), col });
          } else {
            const vs = [];
            for (const [c, v] of e.g) {
              if (c === '10') vs.push({ x: parseFloat(v), y: 0, b: 0 });
              else if (c === '20' && vs.length) vs[vs.length - 1].y = parseFloat(v);
              else if (c === '42' && vs.length) vs[vs.length - 1].b = parseFloat(v);
            }
            const closed = (parseInt(get(e, '70') || '0', 10) & 1) === 1;
            const n = closed ? vs.length : vs.length - 1;
            for (let i = 0; i < n; i++) {
              const a = vs[i], b = vs[(i + 1) % vs.length];
              pushArcBulge([a.x, a.y], [b.x, b.y], a.b, col);
            }
          }
          break;
        case 'POINT':
          if (col === 'other') { ign('POINT màu khác'); break; }
          P.points.push({ x: num(e, '10'), y: num(e, '20'), col });
          break;
        case 'POLYLINE': case 'SPLINE': case 'ELLIPSE': case 'INSERT':
          if (dashed) { ign('đường nét đứt (đường uốn)'); break; }
          throw new NCError('DXF có đối tượng ' + e.type + ' (chưa hỗ trợ). Hãy Explode / chuyển thành LINE + ARC trước.');
        default: break;   // TEXT, MTEXT, DIMENSION, HATCH...: bỏ qua
      }
    }
    for (const k in ignored) P.warnings.push('Bỏ qua ' + ignored[k] + ' ' + k);
    return P;
  }

  // ---------- Hình học ----------
  const dirSeg = (s, rev) => rev ? Object.assign({}, s, { p: s.q, q: s.p, ccw: !s.ccw }) : Object.assign({}, s);
  const reverseLoop = L => L.slice().reverse().map(s => dirSeg(s, true));

  function loopArea(loop) {   // diện tích có dấu, dương = CCW
    let s = 0;
    for (const g of loop) s += g.p[0] * g.q[1] - g.q[0] * g.p[1];
    s /= 2;
    for (const g of loop) if (g.t === 'A') {
      const seg = g.r * g.r / 2 * (g.sweep - Math.sin(g.sweep));
      s += g.ccw ? seg : -seg;
    }
    return s;
  }

  function arcPoints(g, n) {
    const a0 = Math.atan2(g.p[1] - g.c[1], g.p[0] - g.c[0]);
    const sw = g.ccw ? g.sweep : -g.sweep;
    const out = [];
    for (let k = 0; k <= n; k++) out.push([g.c[0] + g.r * Math.cos(a0 + sw * k / n), g.c[1] + g.r * Math.sin(a0 + sw * k / n)]);
    return out;
  }

  function loopBBox(loop) {
    let x0 = Infinity, y0 = Infinity, x1 = -Infinity, y1 = -Infinity;
    const add = p => { x0 = Math.min(x0, p[0]); x1 = Math.max(x1, p[0]); y0 = Math.min(y0, p[1]); y1 = Math.max(y1, p[1]); };
    for (const g of loop) {
      add(g.p); add(g.q);
      if (g.t === 'A') {
        const a0 = Math.atan2(g.p[1] - g.c[1], g.p[0] - g.c[0]);
        for (let k = 0; k < 4; k++) {
          const ang = k * Math.PI / 2;
          const d = g.ccw ? mod(ang - a0, TAU) : mod(a0 - ang, TAU);
          if (d > 1e-9 && d < g.sweep - 1e-9) add([g.c[0] + g.r * Math.cos(ang), g.c[1] + g.r * Math.sin(ang)]);
        }
      }
    }
    return { x0, y0, x1, y1 };
  }

  function groupLoops(segs) {
    const par = segs.map((_, i) => i);
    const f = i => { while (par[i] !== i) { par[i] = par[par[i]]; i = par[i]; } return i; };
    const first = new Map();
    segs.forEach((g, i) => {
      for (const k of [key(g.p), key(g.q)]) {
        if (first.has(k)) par[f(i)] = f(first.get(k)); else first.set(k, i);
      }
    });
    const grp = new Map();
    segs.forEach((g, i) => { const r = f(i); if (!grp.has(r)) grp.set(r, []); grp.get(r).push(g); });
    return [...grp.values()];
  }

  function endpoints(L) {   // các đầu tự do (bậc 1)
    const deg = new Map();
    for (const g of L) for (const p of [g.p, g.q]) { const k = key(p); const e = deg.get(k) || { n: 0, p }; e.n++; deg.set(k, e); }
    return [...deg.values()].filter(e => e.n === 1).map(e => e.p);
  }
  const isOpen = L => endpoints(L).length > 0;

  function chain(segs) {   // nối thành biên kín, chiều kim đồng hồ
    const out = [dirSeg(segs[0], false)], used = new Set([0]);
    while (used.size < segs.length) {
      const last = out[out.length - 1]; let ok = false;
      for (let i = 0; i < segs.length; i++) {
        if (used.has(i)) continue;
        const s = segs[i];
        if (key(s.p) === key(last.q)) out.push(dirSeg(s, false));
        else if (key(s.q) === key(last.q)) out.push(dirSeg(s, true));
        else continue;
        used.add(i); ok = true; break;
      }
      if (!ok) throw new NCError('Biên dạng bị hở hoặc rẽ nhánh tại (' + fmt(last.q[0]) + ', ' + fmt(last.q[1]) + ')');
    }
    if (key(out[0].p) !== key(out[out.length - 1].q)) throw new NCError('Biên dạng không kín');
    return loopArea(out) > 0 ? reverseLoop(out) : out;
  }

  function chainOpenFrom(L, start) {   // nối đường hở bắt đầu từ đầu "start"
    const seq = []; let cur = start; const rest = L.slice();
    while (rest.length) {
      let ok = false;
      for (let i = 0; i < rest.length; i++) {
        const g = rest[i];
        if (key(g.p) === key(cur)) { seq.push(dirSeg(g, false)); cur = g.q; }
        else if (key(g.q) === key(cur)) { seq.push(dirSeg(g, true)); cur = g.p; }
        else continue;
        rest.splice(i, 1); ok = true; break;
      }
      if (!ok) throw new NCError('Đường hở bị đứt tại (' + fmt(cur[0]) + ', ' + fmt(cur[1]) + ')');
    }
    return seq;
  }

  function inside(pt, loop) {
    const pts = [];
    for (const g of loop) {
      if (g.t === 'A') pts.push(...arcPoints(g, 12).slice(0, -1)); else pts.push(g.p);
    }
    const [x, y] = pt; let c = false;
    for (let i = 0; i < pts.length; i++) {
      const [x1, y1] = pts[i], [x2, y2] = pts[(i + 1) % pts.length];
      if ((y1 > y) !== (y2 > y) && x < x1 + (y - y1) * (x2 - x1) / (y2 - y1)) c = !c;
    }
    return c;
  }

  function onSeg(g, pt, tol) {
    if (g.t !== 'L') return false;
    const L = dist(g.p, g.q); if (L < 1e-9) return false;
    const t = ((pt[0] - g.p[0]) * (g.q[0] - g.p[0]) + (pt[1] - g.p[1]) * (g.q[1] - g.p[1])) / (L * L);
    if (t < -1e-9 || t > 1 + 1e-9) return false;
    return dist(pt, [g.p[0] + t * (g.q[0] - g.p[0]), g.p[1] + t * (g.q[1] - g.p[1])]) < tol;
  }

  // Điểm kéo dài ra ngoài biên (kegaki hở) theo pháp tuyến ngoài của biên (CW -> bên trái hướng chạy)
  function extendOut(e, outer, d) {
    for (const g of outer) {
      if (onSeg(g, e, 1e-3)) {
        const L = dist(g.p, g.q), nx = -(g.q[1] - g.p[1]) / L, ny = (g.q[0] - g.p[0]) / L;
        return [e[0] + nx * d, e[1] + ny * d];
      }
    }
    return null;
  }

  // Xoay vòng lỗ (CCW) để bắt đầu tại điểm vào: trái nhất (lỗ ngang) hoặc thấp nhất (lỗ đứng)
  function splitAtEntry(loop) {
    const xs = loop.flatMap(g => [g.p[0], g.q[0]]), ys = loop.flatMap(g => [g.p[1], g.q[1]]);
    const ax = (Math.max(...xs) - Math.min(...xs)) >= (Math.max(...ys) - Math.min(...ys)) ? 0 : 1;
    const target = ax === 0 ? Math.PI : -Math.PI / 2;
    let best = null;
    loop.forEach((g, i) => {
      const cand = [[g.p, null]];
      if (g.t === 'A') {
        const ap = Math.atan2(g.p[1] - g.c[1], g.p[0] - g.c[0]);
        const d = g.ccw ? mod(target - ap, TAU) : mod(ap - target, TAU);
        if (d > 1e-9 && d < g.sweep - 1e-9) cand.push([[g.c[0] + g.r * Math.cos(target), g.c[1] + g.r * Math.sin(target)], d]);
      } else if ((ax === 0 && Math.abs(g.p[0] - g.q[0]) < 1e-9) || (ax === 1 && Math.abs(g.p[1] - g.q[1]) < 1e-9)) {
        cand.push([[(g.p[0] + g.q[0]) / 2, (g.p[1] + g.q[1]) / 2], 'mid']);
      }
      for (const [pt, extra] of cand) {
        const k1 = Math.round(pt[ax] * 1e6) / 1e6, k2 = extra !== null ? 0 : 1;
        if (!best || k1 < best.k1 || (k1 === best.k1 && k2 < best.k2)) best = { k1, k2, i, pt, extra };
      }
    });
    const { i, pt, extra } = best, g = loop[i];
    let out, r = null;
    if (extra === null) out = loop.slice(i).concat(loop.slice(0, i));
    else if (g.t === 'A') {
      const g1 = Object.assign({}, g, { q: pt, sweep: extra }), g2 = Object.assign({}, g, { p: pt, sweep: g.sweep - extra });
      out = [g2].concat(loop.slice(i + 1), loop.slice(0, i), [g1]); r = g.r;
    } else {
      out = [Object.assign({}, g, { p: pt })].concat(loop.slice(i + 1), loop.slice(0, i), [Object.assign({}, g, { q: pt })]);
    }
    return { loop: out, st: pt, r, ax };
  }

  // ---------- Biến đổi ----------
  function mapPrims(P, f) {
    return {
      warnings: P.warnings,
      segs: P.segs.map(s => Object.assign({}, s, { p: f(s.p), q: f(s.q), c: s.c ? f(s.c) : undefined })),
      circles: P.circles.map(c => { const q = f([c.cx, c.cy]); return Object.assign({}, c, { cx: q[0], cy: q[1] }); }),
      points: P.points.map(p => { const q = f([p.x, p.y]); return Object.assign({}, p, { x: q[0], y: q[1] }); })
    };
  }
  const rotate = (P, k) => k === 0 ? P : mapPrims(P, p => { let [x, y] = p; for (let i = 0; i < k; i++) [x, y] = [-y, x]; return [x, y]; });
  const translate = (P, dx, dy) => mapPrims(P, p => [p[0] + dx, p[1] + dy]);

  function analyze(P) {
    const base = P.segs.filter(s => s.col === 'L');
    const loops = groupLoops(base);
    const closed = loops.filter(L => !isOpen(L));
    if (!closed.length) throw new NCError('Không tìm thấy biên dạng ngoài kín (LINE/ARC Layer 0, by layer)');
    const area = L => { const b = loopBBox(L); return (b.x1 - b.x0) * (b.y1 - b.y0); };
    closed.sort((a, b) => area(b) - area(a));
    const outer = chain(closed[0]);
    return { outer, bb: loopBBox(outer), holes: closed.slice(1), opens: loops.filter(isOpen) };
  }

  // ---------- Tạo NC ----------
  function generate(prims0, name, group, t) {
    const { pierceOut, jointOut, pierceHole } = params(group, t);
    const warnings = prims0.warnings.slice();

    // chọn hướng + dời gốc: cần cạnh thẳng đứng sát Oy
    let ch = null;
    for (let rot = 0; rot < 4 && !ch; rot++) {
      let P = rotate(prims0, rot), A = analyze(P), shift = [0, 0];
      if (Math.abs(A.bb.x0) > 1e-4 || Math.abs(A.bb.y0) > 1e-4) {
        shift = [-A.bb.x0, -A.bb.y0]; P = translate(P, shift[0], shift[1]); A = analyze(P);
      }
      if (A.outer.some(g => g.t === 'L' && Math.abs(g.p[0]) < 1e-4 && Math.abs(g.q[0]) < 1e-4 && Math.abs(g.p[1] - g.q[1]) > 1e-6))
        ch = { P, A, rot, shift };
    }
    if (!ch) throw new NCError('Không có cạnh thẳng đứng sát Oy (kể cả khi xoay 90/180/270°). Kiểm tra lại hình dạng phôi.');
    if (ch.rot) warnings.push('Đã xoay hình ' + ch.rot * 90 + '° (ngược chiều kim đồng hồ) để có cạnh thẳng sát Oy');
    if (Math.abs(ch.shift[0]) > 1e-4 || Math.abs(ch.shift[1]) > 1e-4)
      warnings.push('Đã dời gốc tọa độ (' + fmt(ch.shift[0]) + ', ' + fmt(ch.shift[1]) + ') để phôi nằm sát Ox/Oy');
    const { P, A } = ch, sg = A.outer, bb = A.bb;

    // ---- gom các đối tượng ----
    const items = { pts16: [], kegaki: [], ptsG: [], slits: [], holes: [] };
    for (const p of P.points) (p.col === 'L' ? items.pts16 : p.col === 'green' ? items.ptsG : null)?.push([p.x, p.y]);
    if (P.points.some(p => p.col === 'red')) warnings.push('POINT màu đỏ: không có quy tắc, đã bỏ qua');

    const circHole = (c, code) => {
      const px = c.r < pierceHole ? c.cx : c.cx - c.r + pierceHole;   // bán kính < piercing -> đục tâm
      return { area: Math.PI * c.r * c.r, pp: [px, c.cy], st: [c.cx - c.r, c.cy], code,
               segs: [{ t: 'A', c: [c.cx, c.cy], r: c.r, p: [c.cx - c.r, c.cy], q: [c.cx - c.r, c.cy], sweep: TAU, ccw: true }], circle: true };
    };
    for (const c of P.circles) {
      if (group.toUpperCase() !== 'SS' && 2 * c.r >= 50 && c.col !== 'red') throw new NCError('Lỗ ≥ φ50 trên SUS/AL/CU cần joint lỗ: chưa hỗ trợ');
      if (c.col === 'red') items.kegaki.push(Object.assign(circHole(c, 9017), { closed: true }));
      else items.holes.push(circHole(c, 9010));
    }
    const loopHole = (L, code) => {
      const ccw = reverseLoop(chain(L));
      const area = Math.abs(loopArea(ccw));
      const r = splitAtEntry(ccw);
      const w = Math.max(...r.loop.flatMap(g => [g.p[r.ax], g.q[r.ax]])) - r.st[r.ax];
      const dpi = (r.r !== null && r.r < pierceHole) ? r.r : Math.min(pierceHole, w / 2);
      const pp = r.ax === 0 ? [r.st[0] + dpi, r.st[1]] : [r.st[0], r.st[1] + dpi];
      return { area, pp, st: r.st, segs: r.loop, code };
    };
    for (const L of A.holes) {
      items.holes.push(loopHole(L, 9010));
      if (group.toUpperCase() !== 'SS') {
        const b = loopBBox(L);
        if (Math.max(b.x1 - b.x0, b.y1 - b.y0) >= 15) warnings.push('Lỗ hình ' + fmt(b.x1 - b.x0) + '×' + fmt(b.y1 - b.y0) + ' trên SUS/AL/CU có thể cần joint lỗ (chưa hỗ trợ)');
      }
    }
    const nearOrigin = L => L.reduce((m, g) => dist([0, 0], g.p) < dist([0, 0], m.p) ? g : m, L[0]);
    // đường màu đỏ -> kegaki
    for (const L of groupLoops(P.segs.filter(s => s.col === 'red'))) {
      if (!isOpen(L)) {
        let seq = chain(L);
        const k = seq.indexOf(nearOrigin(seq));        // đỉnh gần gốc (0,0) nhất
        seq = seq.slice(k).concat(seq.slice(0, k));
        items.kegaki.push({ pp: seq[0].p, segs: seq, code: 9017, closed: true, g41: true });
      } else {
        const ends = endpoints(L);
        if (ends.length !== 2) throw new NCError('Đường kegaki (đỏ) không phải 1 đường hở đơn');
        const cands = ends.map(e => ({ e, ext: extendOut(e, sg, 3) }));
        const touching = cands.filter(c => c.ext);
        const c = (touching.length ? touching : cands).reduce((m, x) => dist([0, 0], x.ext || x.e) < dist([0, 0], m.ext || m.e) ? x : m);
        let seq = chainOpenFrom(L, c.e);
        if (c.ext) seq = [{ t: 'L', p: c.ext, q: c.e, ccw: true }].concat(seq);
        items.kegaki.push({ pp: c.ext || c.e, segs: seq, code: 9017, g41: true });
      }
    }
    // đường hở by layer + xanh -> khe cắt ; vòng kín xanh -> như lỗ
    const slitSegs = P.segs.filter(s => s.col === 'green');
    for (const L of groupLoops(slitSegs)) {
      if (isOpen(L)) items.slits.push(L); else items.holes.push(loopHole(L, 9010));
    }
    for (const L of A.opens) { items.slits.push(L); warnings.push('Có đường hở Layer 0 by layer: xử lý như khe cắt (M98P9010)'); }
    const slitObjs = items.slits.map(L => {
      const ends = endpoints(L);
      if (ends.length !== 2) throw new NCError('Khe cắt không phải 1 đường hở đơn');
      const out = ends.filter(e => !inside(e, sg));
      if (out.length !== 1) warnings.push('Khe cắt có ' + (out.length === 0 ? 'cả 2 đầu nằm trong' : 'cả 2 đầu nằm ngoài') + ' chi tiết: chọn đầu ' + fmt(ends[0][0]) + ',' + fmt(ends[0][1]) + ' để đục, hãy kiểm tra');
      const st = out.length === 1 ? out[0] : ends[0];
      return { pp: st, segs: chainOpenFrom(L, st), code: 9010, g41: true };
    });

    // ---- phát NC ----
    const body = [], steps = [], xs = [bb.x0, bb.x1], ys = [bb.y0, bb.y1];
    let pos = [0, 0], cut = 0, rapid = 0, beam = 0;
    const pierce = (pt, code) => {
      rapid += dist(pos, pt);
      body.push('G00X' + fmt(pt[0]) + 'Y' + fmt(pt[1]) + 'G40', 'M98P' + code);
      beam++; xs.push(pt[0]); ys.push(pt[1]); steps.push({ x: pt[0], y: pt[1], code }); pos = pt;
    };
    const endCut = () => body.push('M121', 'M199');
    const emit = (segs, prev, g41) => {
      segs.forEach((g, i) => {
        const q = g.q, pre = (g41 && i === 0) ? 'G41' : '';
        if (g.t === 'L') { body.push('G01' + pre + 'X' + fmt(q[0]) + 'Y' + fmt(q[1])); cut += dist(prev, q); }
        else {
          const i_ = g.c[0] - prev[0], j_ = g.c[1] - prev[1];
          let w = (g.ccw ? 'G03' : 'G02') + pre + 'X' + fmt(q[0]) + 'Y' + fmt(q[1]);
          if (Math.abs(Math.round(i_ * 1000)) > 0) w += 'I' + fmt(i_);
          if (Math.abs(Math.round(j_ * 1000)) > 0) w += 'J' + fmt(j_);
          body.push(w); cut += g.r * g.sweep;
        }
        prev = q;
      });
      return prev;
    };
    const nearest = (arr, getPt) => arr.reduce((m, x) => dist(pos, getPt(x)) < dist(pos, getPt(m)) ? x : m);
    const takeAll = (arr, getPt, fn) => { const rem = arr.slice(); while (rem.length) { const x = nearest(rem, getPt); rem.splice(rem.indexOf(x), 1); fn(x); } };

    // 1) lỗ trắng 9016   2) kegaki 9017   3) piercing xuyên (xanh)   4) khe cắt
    takeAll(items.pts16, p => p, p => { pierce(p, 9016); endCut(); });
    takeAll(items.kegaki, k => k.pp, k => {
      pierce(k.pp, 9017);
      if (k.circle) {
        body.push('G01G41X' + fmt(k.st[0]) + 'Y' + fmt(k.st[1])); cut += dist(k.pp, k.st);
        body.push('G03X' + fmt(k.st[0]) + 'Y' + fmt(k.st[1]) + 'I' + fmt(k.segs[0].r)); cut += TAU * k.segs[0].r;
        pos = k.st;
      } else { pos = emit(k.segs, k.pp, true); }
      endCut();
    });
    takeAll(items.ptsG, p => p, p => { pierce(p, 9010); endCut(); });
    takeAll(slitObjs, s => s.pp, s => { pierce(s.pp, 9010); pos = emit(s.segs, s.pp, true); endCut(); });

    // 5) lỗ: diện tích nhỏ -> lớn ; cùng diện tích chọn lỗ gần nhất
    const rem = items.holes.slice().sort((a, b) => a.area - b.area);
    while (rem.length) {
      const grp = rem.filter(h => Math.abs(h.area - rem[0].area) < 1e-3);
      const h = nearest(grp, x => x.pp);
      rem.splice(rem.indexOf(h), 1);
      pierce(h.pp, h.code);
      body.push('G01G41X' + fmt(h.st[0]) + 'Y' + fmt(h.st[1])); cut += dist(h.pp, h.st);
      if (h.circle) { body.push('G03X' + fmt(h.st[0]) + 'Y' + fmt(h.st[1]) + 'I' + fmt(h.segs[0].r)); cut += TAU * h.segs[0].r; }
      else emit(h.segs, h.st, false);
      endCut(); pos = h.st;
    }

    // 6) biên ngoài (CW, G41), piercing ngoài + joint ngoài
    const xmin = bb.x0;
    const cand = [];
    sg.forEach((g, i) => { if (g.t === 'L' && Math.abs(g.p[0] - xmin) < 1e-4 && Math.abs(g.q[0] - xmin) < 1e-4) cand.push([Math.abs(g.q[1] - g.p[1]), i]); });
    cand.sort((a, b) => a[0] - b[0] || a[1] - b[1]);
    let [Lr, i0] = cand[cand.length - 1];
    let a = sg[i0].p, b = sg[i0].q, start, end, pp;
    if (Lr < 10) {   // quy tắc 14/10/2026: cạnh trái < 10 -> đục ở cạnh nằm trên Ox
      const cb = [];
      sg.forEach((g, k) => { if (g.t === 'L' && Math.abs(g.p[1] - bb.y0) < 1e-4 && Math.abs(g.q[1] - bb.y0) < 1e-4) cb.push([Math.abs(g.q[0] - g.p[0]), k]); });
      if (!cb.length) throw new NCError('Cạnh trái < 10 và không có cạnh thẳng trên Ox để đục piercing ngoài');
      cb.sort((p, q) => p[0] - q[0] || p[1] - q[1]);
      const [Lb, ib] = cb[cb.length - 1]; i0 = ib;
      a = sg[i0].p; b = sg[i0].q;
      start = [Math.min(a[0], b[0]) + Math.min(40, Lb / 2), bb.y0];
      const dx = (b[0] - a[0]) / Lb;
      end = [start[0] - dx * jointOut, bb.y0];
      pp = [start[0], start[1] - pierceOut];
    } else {
      const d = b[1] > a[1] ? 1 : -1;
      const sy = Lr <= 100 + 1e-9 ? (a[1] + b[1]) / 2 : Math.min(a[1], b[1]) + 40;
      start = [xmin, sy]; end = [xmin, sy - d * jointOut]; pp = [xmin - pierceOut, sy];
    }
    const seq = []; for (let k = 1; k < sg.length; k++) seq.push(sg[(i0 + k) % sg.length]);
    pierce(pp, 9010);
    body.push('G01G41X' + fmt(start[0]) + 'Y' + fmt(start[1])); cut += pierceOut;
    body.push('G01X' + fmt(b[0]) + 'Y' + fmt(b[1])); cut += dist(start, b);
    const last = emit(seq, b, false);
    body.push('G01X' + fmt(end[0]) + 'Y' + fmt(end[1])); cut += dist(last, end);
    body.push('M121', 'M199', 'G01G40', 'M30');

    const w = Math.max(...xs) - Math.min(...xs), h = Math.max(...ys) - Math.min(...ys);
    const f81 = v => v.toFixed(1).padStart(8);
    const text = [
      '(#_)%(#_)',
      '(' + name.toUpperCase() + '.DXF / ' + w.toFixed(1) + ' X ' + h.toFixed(1) + ')',
      '#501=106', 'M66', 'G90', 'G92X0.Y0.'
    ].concat(body, [
      '(CUTTING_LENGTH = ' + f81(cut) + 'MM  RAPID_LENGTH = ' + f81(rapid) + 'MM)',
      '(TOTAL_LENGTH = ' + f81(cut + rapid) + 'MM)',
      '(BEAM_ON = ' + beam + '  KEGAKI_ON = 0  PREPIERCE_ON = 0)',
      '(#_)%(#_)'
    ]).join('\r\n');

    // dữ liệu vẽ xem trước
    const pv = { lines: [], circles: P.circles, points: P.points, steps, bbox: [Math.min(...xs), Math.min(...ys), Math.max(...xs), Math.max(...ys)] };
    for (const s of P.segs) pv.lines.push({ col: s.col, pts: s.t === 'L' ? [s.p, s.q] : arcPoints(s, Math.max(4, Math.ceil(s.sweep * 8))) });
    return { text, warnings, stats: { cut, rapid, total: cut + rapid, beam, w, h }, preview: pv, params: { pierceOut, jointOut, pierceHole } };
  }

  function convert(dxfInput, name, group, t) {
    return generate(parseDXF(dxfInput), name, group, t);
  }

  root.NCCore = { parseDXF, generate, convert, params, fmt, NCError, TABLE };
})(typeof window !== 'undefined' ? window : globalThis);
