﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlatoCore.Data.Abstractions;
using PlatoCore.Models.Roles;
using PlatoCore.Models.Users;
using PlatoCore.Repositories.Roles;

namespace PlatoCore.Repositories.Users
{

    public class UserRolesRepository : IUserRolesRepository<UserRole>
    {

        #region "Constructor"

        private readonly IDbContext _dbContext;
        private readonly IRoleRepository<Role> _rolesRepository;
        private readonly ILogger<UserSecretRepository> _logger;

        public UserRolesRepository(
            IDbContext dbContext,
            IRoleRepository<Role> rolesRepository,
            ILogger<UserSecretRepository> logger)
        {
            _dbContext = dbContext;
            _rolesRepository = rolesRepository;
            _logger = logger;
        }

        #endregion
        
        #region "Implementation"
        
        public async Task<bool> DeleteAsync(int id)
        {
            bool success;
            using (var context = _dbContext)
            {
                success = await context.ExecuteScalarAsync<bool>(
                    CommandType.StoredProcedure,
                    "DeleteUserRoleById",
                    new IDbDataParameter[]
                    {
                        new DbParam("Id", DbType.Int32, id)
                    });
            }
            return success;
        }

        public async Task<UserRole> InsertUpdateAsync(UserRole userRole)
        {

            // Validate

            if (userRole.UserId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userRole.UserId));
            }

            if (userRole.RoleId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userRole.RoleId));
            }

            // Insert / Update

            var id = 0;
            id = await InsertUpdateInternal(
                userRole.Id,
                userRole.UserId,
                userRole.RoleId);

            if (id > 0)
                return await SelectByIdAsync(id);

            return null;
        }

        public async Task<UserRole> SelectByIdAsync(int id)
        {
            UserRole userRole = null;
            using (var context = _dbContext)
            {
                userRole = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectUserRoleById",
                    async reader =>
                    {
                        if ((reader != null) && (reader.HasRows))
                        {
                            userRole = new UserRole();
                            await reader.ReadAsync();
                            userRole.PopulateModel(reader);
                        }

                        return userRole;
                    }, new IDbDataParameter[]
                    {
                        new DbParam("Id", DbType.Int32, id)
                    });

            }

            return userRole;
        }

        public async Task<IEnumerable<UserRole>> SelectUserRolesByUserId(int userId)
        {
            IList<UserRole> userRoles = null;
            using (var context = _dbContext)
            {
                userRoles = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectUserRolesByUserId",
                    async reader =>
                    {
                        if ((reader != null) && (reader.HasRows))
                        {
                            userRoles = new List<UserRole>();
                            while (await reader.ReadAsync())
                            {
                                var userRole = new UserRole();
                                userRole.PopulateModel(reader);
                                userRoles.Add(userRole);
                            }

                        }

                        return userRoles;
                    }, new IDbDataParameter[]
                    {
                        new DbParam("UserId", DbType.Int32, userId)
                    });

            }

            return userRoles;
        }
        
        public async Task<IEnumerable<UserRole>> InsertUserRolesAsync(
            int userId, IEnumerable<string> roleNames)
        {

            List<UserRole> userRoles = null;
            foreach (var roleName in roleNames)
            {
                var role = _rolesRepository.SelectByNameAsync(roleName);
                if (role != null)
                {
                    var userRole = await InsertUpdateAsync(new UserRole()
                    {
                        UserId = userId,
                        RoleId = role.Id
                    });
                    if (userRoles == null)
                        userRoles = new List<UserRole>();
                    userRoles.Add(userRole);
                }
            }

            return userRoles;

        }

        public async Task<IEnumerable<UserRole>> InsertUserRolesAsync(int userId, IEnumerable<int> roleIds)
        {
            List<UserRole> userRoles = null;
            foreach (var roleId in roleIds)
            {
                var role = _rolesRepository.SelectByIdAsync(roleId);
                if (role != null)
                {
                    var userRole = await InsertUpdateAsync(new UserRole()
                    {
                        UserId = userId,
                        RoleId = role.Id
                    });
                    if (userRoles == null)
                        userRoles = new List<UserRole>();
                    userRoles.Add(userRole);
                }
            }

            return userRoles;
        }
        
        public async Task<bool> DeleteUserRolesAsync(int userId)
        {
            bool success;
            using (var context = _dbContext)
            {
                success = await context.ExecuteScalarAsync<bool>(
                    CommandType.StoredProcedure,
                    "DeleteUserRolesByUserId",
                    new IDbDataParameter[]
                    {
                        new DbParam("UserId", DbType.Int32, userId)
                    });
            }
            return success;
        }
        
        public async Task<bool> DeleteUserRole(int userId, int roleId)
        {

            var success = 0;
            using (var context = _dbContext)
            {
                success = await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "DeleteUserRoleByUserIdAndRoleId",
                    new IDbDataParameter[]
                    {
                        new DbParam("UserId", DbType.Int32, userId),
                        new DbParam("RoleId", DbType.Int32, roleId)
                    });
            }

            return success > 0 ? true : false;

        }
        
        public async Task<IPagedResults<UserRole>> SelectAsync(IDbDataParameter[] dbParams)
        {
            IPagedResults<UserRole> output = null;
            using (var context = _dbContext)
            {
                output = await context.ExecuteReaderAsync<IPagedResults<UserRole>>(
                    CommandType.StoredProcedure,
                    "SelectUserRolesPaged",
                    async reader =>
                    {
                        if ((reader != null) && (reader.HasRows))
                        {
                            output = new PagedResults<UserRole>();
                            while (await reader.ReadAsync())
                            {
                                var userRole = new UserRole();
                                userRole.PopulateModel(reader);
                                output.Data.Add(userRole);
                            }

                            if (await reader.NextResultAsync())
                            {
                                if (reader.HasRows)
                                {
                                    await reader.ReadAsync();
                                    output.PopulateTotal(reader);
                                }
                             
                            }
                        }

                        return output;
                    },
                    dbParams);

            }

            return output;

        }
        
        #endregion

        #region "Private Methods"

        async Task<int> InsertUpdateInternal(
            int id,
            int userId,
            int roleId)
        {
            using (var context = _dbContext)
            {
                return await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "InsertUpdateUserRole",
                    new IDbDataParameter[]
                    {
                        new DbParam("Id", DbType.Int32, id),
                        new DbParam("UserId", DbType.Int32, userId),
                        new DbParam("RoleId", DbType.Int32, roleId),
                        new DbParam("UniqueId", DbType.Int32, ParameterDirection.Output),
                    });
            }
        }
        
        #endregion

    }

}