using Dapper;
using SeatReservation.Application.DataBase;

namespace SeatReservation.Application.Departments.Queries;

public class GetDepartmentsHandler
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetDepartmentsHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }


    public async Task Handle(CancellationToken cancellationToken)
    {
        var recursiveRotts = await GetHierarchyRecursiveAsync("head-office", cancellationToken);
        
        return;
    }

    private async Task<List<DepartmentDto>> GetHierarchyRecursiveAsync(string rootPath, CancellationToken cancellationToken)
    {
        const string dapperSql = """
                                    with recursive dept_tree as (
                                 select d.*, 0 as level
                                 from departments d
                                 where d.path = @rootPath
                                 union all
                                 select c.*, dt.level + 1 as level
                                 from departments c
                                 join dept_tree dt on c.parent_id = dt.id)
                                 select id,
                                        parent_id,
                                        name,
                                        identifier,
                                        path,
                                        depth,
                                        is_active,
                                        created_at,
                                        updated_at,
                                        level
                                     from dept_tree
                                         order by level, id
                                 """;

        var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var departmentRaws = (await connection.QueryAsync<DepartmentDto>(dapperSql, new
        {
            rootPath = rootPath
        })).ToList();
        
        return new List<DepartmentDto>();
    }
}