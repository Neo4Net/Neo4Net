/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.enterprise.builtinprocs
{

	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using BuiltInProcedures = Neo4Net.Kernel.builtinprocs.BuiltInProcedures;
	using IndexProcedures = Neo4Net.Kernel.builtinprocs.IndexProcedures;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using Context = Neo4Net.Procedure.Context;
	using Description = Neo4Net.Procedure.Description;
	using Name = Neo4Net.Procedure.Name;
	using Procedure = Neo4Net.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.procedure.Mode.SCHEMA;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public class EnterpriseBuiltInProcedures
	public class EnterpriseBuiltInProcedures
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public Neo4Net.kernel.api.KernelTransaction tx;
		 public KernelTransaction Tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public Neo4Net.graphdb.DependencyResolver resolver;
		 public DependencyResolver Resolver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Create a node key constraint with index backed by specified index provider " + "(for example: CALL db.createNodeKey(\":Person(name)\", \"lucene+native-2.0\")) - " + "YIELD index, providerName, status") @Procedure(name = "db.createNodeKey", mode = SCHEMA) public java.util.stream.Stream<Neo4Net.kernel.builtinprocs.BuiltInProcedures.SchemaIndexInfo> createNodeKey(@Name("index") String index, @Name("providerName") String providerName) throws Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
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