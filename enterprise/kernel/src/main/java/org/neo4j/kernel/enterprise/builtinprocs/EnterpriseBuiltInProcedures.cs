/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Kernel.enterprise.builtinprocs
{

	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using BuiltInProcedures = Org.Neo4j.Kernel.builtinprocs.BuiltInProcedures;
	using IndexProcedures = Org.Neo4j.Kernel.builtinprocs.IndexProcedures;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using Context = Org.Neo4j.Procedure.Context;
	using Description = Org.Neo4j.Procedure.Description;
	using Name = Org.Neo4j.Procedure.Name;
	using Procedure = Org.Neo4j.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.Mode.SCHEMA;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public class EnterpriseBuiltInProcedures
	public class EnterpriseBuiltInProcedures
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.kernel.api.KernelTransaction tx;
		 public KernelTransaction Tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.graphdb.DependencyResolver resolver;
		 public DependencyResolver Resolver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Create a node key constraint with index backed by specified index provider " + "(for example: CALL db.createNodeKey(\":Person(name)\", \"lucene+native-2.0\")) - " + "YIELD index, providerName, status") @Procedure(name = "db.createNodeKey", mode = SCHEMA) public java.util.stream.Stream<org.neo4j.kernel.builtinprocs.BuiltInProcedures.SchemaIndexInfo> createNodeKey(@Name("index") String index, @Name("providerName") String providerName) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Create a node key constraint with index backed by specified index provider " + "(for example: CALL db.createNodeKey(\":Person(name)\", \"lucene+native-2.0\")) - " + "YIELD index, providerName, status"), Procedure(name : "db.createNodeKey", mode : SCHEMA)]
		 public virtual Stream<BuiltInProcedures.SchemaIndexInfo> CreateNodeKey( string index, string providerName )
		 {
			  using ( IndexProcedures indexProcedures = indexProcedures() )
			  {
					return indexProcedures.CreateNodeKey( index, providerName );
			  }
		 }

		 private IndexProcedures IndexProcedures()
		 {
			  return new IndexProcedures( Tx, Resolver.resolveDependency( typeof( IndexingService ) ) );
		 }
	}

}