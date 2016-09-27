using System;
using System.Collections.Generic;
using System.Linq;
using Nest;

namespace ElasticSearchDemo
{
    public class Employee
    {
        public int EmpId { set; get; }

        public string Name { set; get; }

        public string Department { set; get; }

        public int Salary { set; get; }
    }
    class Program
    {
        public static Uri EsNode;
        public static ConnectionSettings EsConfig;
        public static ElasticClient EsClient;
        static void Main(string[] args)
        {
            EsNode = new Uri("http://localhost:9200/");
            EsConfig = new ConnectionSettings(EsNode).DefaultIndex("employee");
            EsClient = new ElasticClient(EsConfig);

            var settings = new IndexSettings { NumberOfReplicas = 1, NumberOfShards = 2 };

            var indexConfig = new IndexState
            {
                Settings = settings
            };

            if (!EsClient.IndexExists("employee").Exists)
            {
                EsClient.CreateIndex("employee", c => c
                    .InitializeUsing(indexConfig)
                    .Mappings(m => m.Map<Employee>(mp => mp.AutoMap()))
                );
            }

            //InsertDocument();
            //InsertBulkDocument();
            //PopulateDocument();
            //UpdateDocument();
            //UpdateDocumentWithNewField();
            //GetDocument();
            //SearchDocumentMethod1();
            //SearchDocumentMethod2();
            //SearchDocumentUsingOROperator();
            //SearchDocumentUsingANDOperator();
            //SearchDocumentUsingNOTOperator();
            //SearchDocumentUsingOperatorOverloading();
            //SearchDocumentUsingFilter();
            //SearchDocumentComplex1();
            //SearchDocumentComplex2();
            //SearchDocumentComplex3();
            //DeleteIndex();
            //DeleteDocument();
        }

        public static void InsertDocument()
        {
            var lst = PopulateEmployees();

            foreach (var obj in lst.Select((value, counter) => new { counter, value }))
            {
                EsClient.Index(obj.value, i => i
                    .Index("employee")
                    .Type("myEmployee")
                    .Id(obj.counter)
                    .Refresh()
                    );
            }

        }

        public static void InsertBulkDocument()
        {
            var descriptor = new BulkDescriptor();

            foreach (var employeeObj in PopulateEmployees())
            {
                Employee obj = employeeObj;
                descriptor.Index<Employee>(op => op.Document(obj));
            }
            var bulkresult = EsClient.Bulk(descriptor);

        }

        public static void PopulateDocument()
        {
            for (var c = 1; c < 25000; c++)
            {
                EsClient.Index(new { EmpId = c, Name = "John" + c, Department = "IT", Salary = 45000 + c },
                    i => i
                    .Index("employee")
                    .Type("myEmployee")
                    .Id(c)
                    );
            }
        }

        public static object SearchDocumentMethod1(string qr = "IT")
        {
            var query = qr.ToLower();
            var response = EsClient.Search<Employee>(s => s
                            .From(0)
                            .Size(10000)
                            .Index("employee")
                            .Type("myEmployee")
                            .Query(q =>
                                    q.Term(t => t.Department, query)
                                   )
                            );
            return response;
        }

        public static object SearchDocumentMethod2(string qr = "IT")
        {
            var response = EsClient.Search<Employee>(s => s
                            .From(0)
                            .Size(10000)
                            .Index("employee")
                            .Type("myEmployee")
                            .Query(q =>
                                    q.Match(mq => mq.Field(f => f.Department).Query(qr))
                                   )
                            );
            return response;
        }

        public static object SearchDocumentUsingOROperator(string dept = "IT", string name = "XYZ")
        {
            var qDept = dept.ToLower();
            var qName = name.ToLower();
            var response = EsClient.Search<Employee>(s => s
                            .From(0)
                            .Size(10000)
                            .Index("employee")
                            .Type("myEmployee")
                            .Query(q => q
                                    .Bool(b => b
                                        .Should(
                                            bs => bs.Term(p => p.Department, dept),
                                            bs => bs.Term(p => p.Name, name)
                                        )
                                    )
                                )
                            );
            return response;
        }

        public static object SearchDocumentUsingANDOperator(string dept = "IT", string name = "XYZ")
        {
            var qDept = dept.ToLower();
            var qName = name.ToLower();
            var response = EsClient.Search<Employee>(s => s
                            .From(0)
                            .Size(10000)
                            .Index("employee")
                            .Type("myEmployee")
                            .Query(q => q
                                    .Bool(b => b
                                        .Must(
                                            bs => bs.Term(p => p.Department, qDept),
                                            bs => bs.Term(p => p.Name, qName)
                                        )
                                    )
                                )
                            );
            return response;
        }

        public static object SearchDocumentUsingNOTOperator(string dept = "IT", int empId = 45)
        {
            var qDept = dept.ToLower();
            var qempId = empId;
            var response = EsClient.Search<Employee>(s => s
                            .From(0)
                            .Size(1)
                            .Index("employee")
                            .Type("myEmployee")
                            .Query(q => q
                                    .Bool(b => b
                                        .MustNot(
                                            bs => bs.Term(p => p.Department, qDept),
                                            bs => bs.Term(p => p.EmpId, qempId)
                                        )
                                    )
                                )
                            );
            return response;
        }

        public static object SearchDocumentUsingOperatorOverloading()
        {
            var qDept = "IT".ToLower();
            var qName = "John1".ToLower();
            var response = EsClient.Search<Employee>(s => s
                            .From(0)
                            .Size(10000)
                            .Index("employee")
                            .Type("myEmployee")
                            .Query(q =>
                                    q.Term(p => p.Name, qName) &&
                                   (q.Term(p => p.Department, qDept) ||
                                    q.Term(p => p.Salary, 45139))
                                )
                            );
            return response;
        }

        public static object SearchDocumentUsingFilter()
        {
            var response = EsClient.Search<Employee>(s => s
                            .From(0)
                            .Size(10000)
                            .Index("employee")
                            .Type("myEmployee")
                            .Query(q => q
                                .Bool(b => b
                                    .Filter(f => f.Range(m => m.Field("salary").LessThan(45139)))
                                    )
                                )
                            );
            return response;
        }

        public static object SearchDocumentComplex1()
        {
            var response = EsClient.Search<Employee>(s => s
                            .From(0)
                            .Size(10000)
                            .Index("employee")
                            .Type("myEmployee")
                            .Query(q => q
                                .Bool(b => b
                                    .Must(
                                        bs => bs.Term(p => p.Salary, "45112"),
                                        bs => bs.Term(p => p.EmpId, "112"),
                                        bs => bs.Range(m => m.Field("salary").LessThanOrEquals(45112))
                                        )
                                    )
                                )
                            );
            return response;
        }

        public static object SearchDocumentComplex2()
        {
            var response = EsClient.Search<Employee>(s => s
                            .From(0)
                            .Size(10000)
                            .Index("employee")
                            .Type("myEmployee")
                            .Query(q => q
                                .Bool(b => b
                                    .Must(
                                        bs => bs.Term(p => p.Salary, "45112"),
                                        bs => bs.Term(p => p.EmpId, "112")
                                        )
                                    .Filter(f => f.Range(m => m.Field("salary").GreaterThanOrEquals(45112)))
                                    )
                                )
                            );
            return response;
        }

        public static object SearchDocumentComplex3()
        {
            var response = EsClient.Search<Employee>(s => s
                            .From(0)
                            .Size(10000)
                            .Index("employee")
                            .Type("myEmployee")
                            .Query(q =>
                                q.Term(p => p.Name, "john150") ||
                                q.Term(p => p.Salary, "45149") ||
                                    (
                                        q.TermRange(p => p.Field(f => f.Salary).GreaterThanOrEquals("45100")) &&
                                        q.TermRange(p => p.Field(f => f.Salary).LessThanOrEquals("45105"))
                                    )
                                )
                            );
            return response;
        }

        public static object DeleteIndex()
        {
            var response = EsClient.DeleteIndex("employee");
            return response;
        }

        public static object DeleteDocument()
        {
            var response = EsClient.Delete<Employee>(2, d => d.Index("employee").Type("myEmployee"));
            return response;
        }

        public static object GetDocument()
        {
            var response = EsClient.Get<Employee>(3, idx => idx.Index("employee").Type("myEmployee"));
            return response.Source;
        }

        public static object UpdateDocument()
        {

            var emp = new Employee { EmpId = 3, Name = "John_up", Department = "IT", Salary = 85000 };
            var response = EsClient.Update(DocumentPath<Employee>
                .Id(3),
                u => u
                    .Index("employee")
                    .Type("myEmployee")
                    .DocAsUpsert(true)
                    .Doc(emp));
            return response;
        }

        public static object UpdateDocumentWithNewField()
        {
            var response = EsClient.Update(DocumentPath<object>
                .Id(3),
                u => u
                    .Index("employee")
                    .Type("myEmployee")
                    .DocAsUpsert(true)
                    .Doc(new { EmpId = 3, Name = "John_up", Department = "IT", Salary = 85000, Country = "USA" }));
            return response;
        }

        public static List<Employee> PopulateEmployees()
        {
            return new List<Employee>
            {
                new Employee {EmpId = 1, Name = "John", Department = "IT", Salary = 45000},
                new Employee {EmpId = 2, Name = "Will", Department = "Dev", Salary = 35000},
                new Employee {EmpId = 3, Name = "Henry", Department = "Dev", Salary = 25000},
                new Employee {EmpId = 4, Name = "Eric", Department = "Dev", Salary = 15000},
                new Employee {EmpId = 5, Name = "Steve", Department = "Dev", Salary = 65000},
                new Employee {EmpId = 6, Name = "Mike", Department = "QA", Salary = 75000},
                new Employee {EmpId = 7, Name = "Mark", Department = "QA", Salary = 55000},
                new Employee {EmpId = 8, Name = "Kevin", Department = "QA", Salary = 45000},
                new Employee {EmpId = 9, Name = "Haddin", Department = "Dev", Salary = 25000},
                new Employee {EmpId = 10, Name = "Smith", Department = "Dev", Salary = 15000}
            };
        }

    }

}
