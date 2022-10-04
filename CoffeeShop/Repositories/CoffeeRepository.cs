using CoffeeShop.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System;

namespace CoffeeShop.Repositories
{
        public class CoffeeRepository : ICoffeeRepository
        {
            private readonly string _connectionString;
            public CoffeeRepository(IConfiguration configuration)
            {
                _connectionString = configuration.GetConnectionString("DefaultConnection");
            }

            private SqlConnection Connection
            {
                get { return new SqlConnection(_connectionString); }
            }

            public List<Coffee> GetAll()
            {
                using (var conn = Connection)
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT c.Id AS CoffeeId, c.[Title] AS CoffeeTitle, b.Id AS BeanId, b.Name AS BeanName, b.Region AS BeanRegion, b.Notes AS BeanNotes FROM Coffee c\r\nLEFT JOIN BeanVariety b ON c.BeanVarietyId = b.Id;";
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var CoffeeVarieties = new List<Coffee>();
                            while (reader.Read())
                            {
                                var variety = new Coffee()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CoffeeId")),
                                    Title = reader.GetString(reader.GetOrdinal("CoffeeTitle")),
                                    BeanVariety = new BeanVariety
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("BeanId")),
                                        Name = reader.GetString(reader.GetOrdinal("BeanName")),
                                        Region = reader.GetString(reader.GetOrdinal("BeanRegion")),
                                    }
                                };
                                if (!reader.IsDBNull(reader.GetOrdinal("BeanNotes")))
                                {
                                    variety.BeanVariety.Notes = reader.GetString(reader.GetOrdinal("BeanNotes"));
                                }
                                CoffeeVarieties.Add(variety);
                            }

                            return CoffeeVarieties;
                        }
                    }
                }
            }

            public Coffee Get(int id)
            {
                using (var conn = Connection)
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                        SELECT c.Id AS CoffeeId,
                        c.[Title] AS CoffeeTitle,
                        b.Id AS BeanId, 
                        b.Name AS BeanName, 
                        b.Region AS BeanRegion, 
                        b.Notes AS BeanNotes 
                        FROM Coffee c
                        LEFT JOIN BeanVariety b ON c.BeanVarietyId = b.Id\
                        WHERE c.Id = @id;";
                        cmd.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            Coffee variety = null;
                            if (reader.Read())
                            {
                                variety = new Coffee()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CoffeeId")),
                                    Title = reader.GetString(reader.GetOrdinal("CoffeeTitle")),
                                    BeanVariety = new BeanVariety
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("BeanId")),
                                        Name = reader.GetString(reader.GetOrdinal("BeanName")),
                                        Region = reader.GetString(reader.GetOrdinal("BeanRegion")),
                                    }
                                };
                                if (!reader.IsDBNull(reader.GetOrdinal("BeanNotes")))
                                {
                                    variety.BeanVariety.Notes = reader.GetString(reader.GetOrdinal("BeanNotes"));
                                }
                            }

                            return variety;
                        }
                    }
                }
            }

            public void Add(Coffee coffee)
            {
                using (var conn = Connection)
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                        INSERT INTO Coffee ([Title], BeanVarietyId)
                        OUTPUT INSERTED.ID
                        VALUES (@title, @beanVarietyId)";
                        cmd.Parameters.AddWithValue("@title", coffee.Title);
                        cmd.Parameters.AddWithValue("@region", coffee.BeanVarietyId);

                        coffee.Id = (int)cmd.ExecuteScalar();
                    }
                }
            }

            public void Update(Coffee coffee)
            {
                using (var conn = Connection)
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                        UPDATE Coffee 
                           SET [Title] = @title, 
                               BeanVarietyId = @BeanVarietyId, 
                         WHERE Id = @id";
                        cmd.Parameters.AddWithValue("@id", coffee.Id);
                        cmd.Parameters.AddWithValue("@title", coffee.Title);
                        cmd.Parameters.AddWithValue("@region", coffee.BeanVarietyId);

                        cmd.ExecuteNonQuery();
                    }
                }
            }

            public void Delete(int id)
            {
                using (var conn = Connection)
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM Coffee WHERE Id = @id";
                        cmd.Parameters.AddWithValue("@id", id);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
}
