
const { getConnection, sql } = require("../config/database");

class Usuario {
  static async criar(dados) {
    const conexao = await getConnection();

    const resultado = await conexao.request()
      .input("Nome", sql.VarChar(100), dados.Nome)
      .input("Matricula", sql.VarChar(30), dados.Matricula)
      .input("Email", sql.VarChar(100), dados.Email)
      .input("SenhaHash", sql.VarChar(255), dados.SenhaHash)
      .input("Perfil", sql.VarChar(20), dados.Perfil || "Aluno")
      .query(`
        INSERT INTO Usuarios
        (Nome, Matricula, Email, SenhaHash, Perfil)
        OUTPUT INSERTED.IdUsuario
        VALUES
        (@Nome, @Matricula, @Email, @SenhaHash, @Perfil)
      `);

    return resultado.recordset[0];
  }

  static async listar() {
    const conexao = await getConnection();

    const resultado = await conexao.request()
      .query(`
        SELECT
          IdUsuario,
          Nome,
          Matricula,
          Email,
          Perfil,
          Status
        FROM Usuarios
        ORDER BY Nome
      `);

    return resultado.recordset;
  }

  static async buscarPorId(id) {
    const conexao = await getConnection();

    const resultado = await conexao.request()
      .input("IdUsuario", sql.Int, id)
      .query(`
        SELECT
          IdUsuario,
          Nome,
          Matricula,
          Email,
          Perfil,
          Status
        FROM Usuarios
        WHERE IdUsuario = @IdUsuario
      `);

    return resultado.recordset[0];
  }

  static async buscarPorMatricula(matricula) {
    const conexao = await getConnection();

    const resultado = await conexao.request()
      .input("Matricula", sql.VarChar(30), matricula)
      .query(`
        SELECT *
        FROM Usuarios
        WHERE Matricula = @Matricula
      `);

    return resultado.recordset[0];
  }

  static async atualizar(id, dados) {
    const conexao = await getConnection();

    const resultado = await conexao.request()
      .input("IdUsuario", sql.Int, id)
      .input("Nome", sql.VarChar(100), dados.Nome)
      .input("Matricula", sql.VarChar(30), dados.Matricula)
      .input("Email", sql.VarChar(100), dados.Email)
      .input("Perfil", sql.VarChar(20), dados.Perfil)
      .query(`
        UPDATE Usuarios
        SET
          Nome = @Nome,
          Matricula = @Matricula,
          Email = @Email,
          Perfil = @Perfil
        WHERE IdUsuario = @IdUsuario;

        SELECT @@ROWCOUNT AS LinhasAfetadas;
      `);

    return resultado.recordset[0].LinhasAfetadas > 0;
  }

  static async inativar(id) {
    const conexao = await getConnection();

    const resultado = await conexao.request()
      .input("IdUsuario", sql.Int, id)
      .query(`
        UPDATE Usuarios
        SET Status = 'Inativo'
        WHERE IdUsuario = @IdUsuario;

        SELECT @@ROWCOUNT AS LinhasAfetadas;
      `);

    return resultado.recordset[0].LinhasAfetadas > 0;
  }
}

module.exports = Usuario;
