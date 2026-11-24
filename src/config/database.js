
const sql = require("mssql");
require("dotenv").config();

const config = {
  user: process.env.DB_USER,
  password: process.env.DB_PASSWORD,
  server: process.env.DB_SERVER,
  database: process.env.DB_DATABASE,
  port: Number(process.env.DB_PORT || 1433),
  options: {
    encrypt: false,
    trustServerCertificate: true
  }
};

async function getConnection() {
  try {
    return await sql.connect(config);
  } catch (erro) {
    console.error("Erro ao conectar ao SQL Server:", erro);
    throw erro;
  }
}

module.exports = {
  sql,
  getConnection
};
