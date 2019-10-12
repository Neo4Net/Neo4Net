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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using Test = org.junit.Test;

	using SchemaWrite = Org.Neo4j.@internal.Kernel.Api.SchemaWrite;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using TestEnterpriseGraphDatabaseFactory = Org.Neo4j.Test.TestEnterpriseGraphDatabaseFactory;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;

	public class IndexCreateEnterpriseIT : IndexCreateIT
	{
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		 private static readonly IndexCreator _nodeKeyCreator = SchemaWrite::nodeKeyConstraintCreate;

		 protected internal override TestGraphDatabaseFactory CreateGraphDatabaseFactory()
		 {
			  return new TestEnterpriseGraphDatabaseFactory();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateNodeKeyConstraintWithSpecificExistingProviderName() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateNodeKeyConstraintWithSpecificExistingProviderName()
		 {
			  ShouldCreateWithSpecificExistingProviderName( _nodeKeyCreator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailCreateNodeKeyWithNonExistentProviderName() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailCreateNodeKeyWithNonExistentProviderName()
		 {
			  ShouldFailWithNonExistentProviderName( _nodeKeyCreator );
		 }
	}

}