﻿using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Kernel.builtinprocs
{
	using Test = org.junit.Test;


	using Org.Neo4j.Collection;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using ProcedureCallContext = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext;
	using QualifiedName = Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName;
	using ResourceTracker = Org.Neo4j.Kernel.api.ResourceTracker;
	using StubResourceManager = Org.Neo4j.Kernel.api.StubResourceManager;
	using KernelIntegrationTest = Org.Neo4j.Kernel.Impl.Api.integrationtest.KernelIntegrationTest;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.toArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.ProcedureSignature.procedureName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.SecurityContext.AUTH_DISABLED;

	public class BuiltInDbmsProceduresIT : KernelIntegrationTest
	{
		 private readonly ResourceTracker _resourceTracker = new StubResourceManager();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListConfig()
		 {
			  // When
			  IList<object[]> config = CallListConfig( "" );
			  IList<string> names = config.Select( o => o[0].ToString() ).ToList();

			  // The size of the config is not fixed so just make sure it's the right magnitude
			  assertTrue( names.Count > 10 );

			  assertThat( names, hasItem( GraphDatabaseSettings.record_format.name() ) );

			  // Should not contain "unsupported.*" configs
			  assertEquals( names.Where( n => n.StartsWith( "unsupported" ) ).Count(), 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listConfigWithASpecificConfigName() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListConfigWithASpecificConfigName()
		 {
			  // When
			  IList<object[]> config = CallListConfig( GraphDatabaseSettings.strict_config_validation.name() );

			  assertEquals( 1, config.Count );
			  assertArrayEquals( new object[]{ "dbms.config.strict_validation", "A strict configuration validation will prevent the database from starting up if unknown " + "configuration options are specified in the neo4j settings namespace (such as dbms., ha., " + "cypher., etc). This is currently false by default but will be true by default in 4.0.", "false", false }, config[0] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void durationAlwaysListedWithUnit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DurationAlwaysListedWithUnit()
		 {
			  // When
			  IList<object[]> config = CallListConfig( GraphDatabaseSettings.transaction_timeout.name() );

			  assertEquals( 1, config.Count );
			  assertArrayEquals( new object[]{ "dbms.transaction.timeout", "The maximum time interval of a transaction within which it should be completed.", "0ms", true }, config[0] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listDynamicSetting() throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListDynamicSetting()
		 {
			  IList<object[]> config = CallListConfig( GraphDatabaseSettings.check_point_iops_limit.name() );

			  assertEquals( 1, config.Count );
			  assertTrue( ( bool? ) config[0][3] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listNotDynamicSetting() throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListNotDynamicSetting()
		 {
			  IList<object[]> config = CallListConfig( GraphDatabaseSettings.data_directory.name() );

			  assertEquals( 1, config.Count );
			  assertFalse( ( bool? ) config[0][3] );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<Object[]> callListConfig(String seatchString) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private IList<object[]> CallListConfig( string seatchString )
		 {
			  QualifiedName procedureName = procedureName( "dbms", "listConfig" );
			  RawIterator<object[], ProcedureException> callResult = DbmsOperations().procedureCallDbms(procedureName, toArray(seatchString), DependencyResolver, AUTH_DISABLED, _resourceTracker, ProcedureCallContext.EMPTY);
			  return new IList<object[]> { callResult };
		 }
	}

}