using Dapper;
using SeatReservation.Application.DataBase;
using SeatReservation.Contracts.Departments;

namespace SeatReservation.Application.Departments.Queries;

public class GetDepartmentLtreeHandler
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetDepartmentLtreeHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<DepartmentDto>> Handle(CancellationToken cancellationToken)
    {
        var recursiveRotts = await GetHierarchyLtreeAsync("head-office.hr", cancellationToken);
        
        return recursiveRotts;
    }
    
    private async Task<List<DepartmentDto>> GetHierarchyLtreeAsync(string rootPath, CancellationToken cancellationToken)
    {
        const string dapperSql = """
                                    select id,
                                        parent_id,
                                        name,
                                        identifier,
                                        path,
                                        depth,
                                        is_active,
                                        created_at,
                                        updated_at
                                    from departments
                                    --where path <@ @rootPath::ltree
                                    --where subpath(path, 0, nlevel(path)-1) = subpath(@rootPath::ltree, 0, nlevel(@rootPath::ltree)-1) and path != @rootPath::ltree
                                    where path <@ @rootPath::ltree
                                     and nlevel(path) > nlevel(@rootPath::ltree)
                                     and nlevel(path) <= nlevel(@rootPath::ltree) + 3
                                    order by depth
                                 """;

        var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        var departmentRaws = (await connection.QueryAsync<DepartmentDto>(dapperSql, new
        {
            rootPath
        })).ToList();

        var departmentsDict = departmentRaws.ToDictionary(x => x.Id);
        var roots = new List<DepartmentDto>();

        foreach (var row in departmentRaws)
        {
            if (row.ParentId.HasValue && departmentsDict.TryGetValue(row.ParentId.Value, out var parent))
            {
                parent.Children.Add(departmentsDict[row.Id]);
            }
            else
            {
                roots.Add(departmentsDict[row.Id]);
            }
        }
        
        return roots;
    }
}