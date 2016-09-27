# ElasticSearch Using Nest C#
ElasticSearch Connection, Configuration, Initializing, DML/DDL operations and Searching queries using NEST in .NET

## Search Queries Example

In order to search specific document use SEARCH API. Please note if type of document has been customized by you during creating index, explicitly mention that type with index for instance (you need to do this if you have not set DefaultIndex API with connection setting object )

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
    
### OR operation:

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
                                        bs => bs.Term(p => p.Department, qDept),
                                        bs => bs.Term(p => p.Name, qName)
                                    )
                                )
                            )
                        );
        return response;
    }

### AND operation:

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

### NOT operation:

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

### Operator Overloading for Boolean operation:

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

### Filter operation:

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
    
### Complex operation:

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

### Notes: 

- A bool query literally combines multiple queries of any type together with clauses such as must, must_not, and should.
- A term query specifies a single field and a single term to determine if the field matches. Note that term queries are specifically for non-analyzed fields.
- Analyzers process the text in order to obtain the terms that are finally indexed/searched. An analyzer of type standard is built using the Standard Tokenizer with the Standard Token Filter, Lower Case Token Filter, and Stop Token Filter. So always convert searching input to lower case for using Term API. Using the Standard Analyzer GET becomes get when stored in the index. The source document will still have the original “GET”. The match query will apply the same standard analyzer to the search term and will therefore match what is stored in the index. The term query does not apply any analyzers to the search term, so will only look for that exact term in the inverted index.To use the term query in your example, change the upper case “GET” to lower case “get” or change your mapping so the request.method field is set to not_analyzed.
- Query DSL : Elasticsearch provides a full Query DSL based on JSON to define queries , consisting of two types of clauses:

1. Leaf query clauses
   Leaf query clauses look for a particular value in a particular field, such as the match, term or range queries. These queries can be used by themselves.
2. Compound query clauses
   Compound query clauses wrap other leaf or compound queries and are used to combine multiple queries in a logical fashion (such as the bool or dis_max query), or to alter their behaviour (such as the not orconstant_score query).
