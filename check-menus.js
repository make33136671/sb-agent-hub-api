const { Client } = require('pg');
const c = new Client({
  host: process.env.DB_HOST || '172.25.167.196',
  port: Number(process.env.DB_PORT || 5432),
  database: process.env.DB_NAME || 'sb_agent_hub',
  user: process.env.DB_USER || 'postgres',
  password: process.env.DB_PASSWORD,
});
(async () => {
  await c.connect();
  const r = await c.query(`SELECT "Id", "ParentId", "Name", "Path", "Component", "OrderNo", "HideInMenu" FROM sb_agenthub_menu WHERE "IsDeleted" = false ORDER BY "OrderNo"`);
  for (const m of r.rows) {
    const indent = m.ParentId > 0 ? '  └─' : '';
    console.log(`${indent} Id=${m.Id} OrderNo=${m.OrderNo} Path=${m.Path} Name=${m.Name} HideInMenu=${m.HideInMenu}`);
  }
  await c.end();
})();
