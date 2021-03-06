namespace Dashing.Engine.DML {
    public interface ICountWriter {
        SqlWriterResult GenerateCountSql<T>(SelectQuery<T> selectQuery) where T : class, new();
    }
}