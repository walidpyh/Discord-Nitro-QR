var req = webpackJsonp.push([
	[], {
		extra_id: (e, t, r) => e.exports = r
	},
	[
		["extra_id"]
	]
]);
for (let e in req.c)
	if (req.c.hasOwnProperty(e)) {
		let t = req.c[e].exports;
		if (t && t.__esModule && t.default)
			for (let e in t.default) "getToken" === e && (token = t.default.getToken())
	}
return token;