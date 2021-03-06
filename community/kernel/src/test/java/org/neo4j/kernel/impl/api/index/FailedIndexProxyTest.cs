﻿/*
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
namespace Org.Neo4j.Kernel.Impl.Api.index
{
	using Test = org.junit.Test;

	using IndexCapability = Org.Neo4j.@internal.Kernel.Api.IndexCapability;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using IndexPopulator = Org.Neo4j.Kernel.Api.Index.IndexPopulator;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptorFactory.forSchema;

	public class FailedIndexProxyTest
	{
		 private readonly IndexProviderDescriptor _providerDescriptor = mock( typeof( IndexProviderDescriptor ) );
		 private readonly IndexCapability _indexCapability = mock( typeof( IndexCapability ) );
		 private readonly IndexPopulator _indexPopulator = mock( typeof( IndexPopulator ) );
		 private readonly IndexPopulationFailure _indexPopulationFailure = mock( typeof( IndexPopulationFailure ) );
		 private readonly IndexCountsRemover _indexCountsRemover = mock( typeof( IndexCountsRemover ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveIndexCountsWhenTheIndexItselfIsDropped() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveIndexCountsWhenTheIndexItselfIsDropped()
		 {
			  // given
			  string userDescription = "description";
			  FailedIndexProxy index = new FailedIndexProxy( forSchema( forLabel( 1, 2 ), IndexProviderDescriptor.UNDECIDED ).withId( 1 ).withoutCapabilities(), userDescription, _indexPopulator, _indexPopulationFailure, _indexCountsRemover, NullLogProvider.Instance );

			  // when
			  index.Drop();

			  // then
			  verify( _indexPopulator ).drop();
			  verify( _indexCountsRemover ).remove();
			  verifyNoMoreInteractions( _indexPopulator, _indexCountsRemover );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogReasonForDroppingIndex() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogReasonForDroppingIndex()
		 {
			  // given
			  AssertableLogProvider logProvider = new AssertableLogProvider();

			  // when
			  ( new FailedIndexProxy( forSchema( forLabel( 0, 0 ), IndexProviderDescriptor.UNDECIDED ).withId( 1 ).withoutCapabilities(), "foo", mock(typeof(IndexPopulator)), IndexPopulationFailure.Failure("it broke"), _indexCountsRemover, logProvider ) ).drop();

			  // then
			  logProvider.AssertAtLeastOnce( inLog( typeof( FailedIndexProxy ) ).info( "FailedIndexProxy#drop index on foo dropped due to:\nit broke" ) );
		 }
	}

}