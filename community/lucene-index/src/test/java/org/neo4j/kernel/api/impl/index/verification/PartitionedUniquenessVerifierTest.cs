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
namespace Org.Neo4j.Kernel.Api.Impl.Index.verification
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Answers = org.mockito.Answers;
	using Mock = org.mockito.Mock;
	using MockitoJUnitRunner = org.mockito.junit.MockitoJUnitRunner;


	using PartitionSearcher = Org.Neo4j.Kernel.Api.Impl.Index.partition.PartitionSearcher;
	using LuceneDocumentStructure = Org.Neo4j.Kernel.Api.Impl.Schema.LuceneDocumentStructure;
	using DuplicateCheckingCollector = Org.Neo4j.Kernel.Api.Impl.Schema.verification.DuplicateCheckingCollector;
	using PartitionedUniquenessVerifier = Org.Neo4j.Kernel.Api.Impl.Schema.verification.PartitionedUniquenessVerifier;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.LuceneTestUtil.valueTupleList;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(MockitoJUnitRunner.class) public class PartitionedUniquenessVerifierTest
	public class PartitionedUniquenessVerifierTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock(answer = org.mockito.Answers.RETURNS_DEEP_STUBS) private org.neo4j.kernel.api.impl.index.partition.PartitionSearcher searcher1;
		 private PartitionSearcher _searcher1;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock(answer = org.mockito.Answers.RETURNS_DEEP_STUBS) private org.neo4j.kernel.api.impl.index.partition.PartitionSearcher searcher2;
		 private PartitionSearcher _searcher2;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock(answer = org.mockito.Answers.RETURNS_DEEP_STUBS) private org.neo4j.kernel.api.impl.index.partition.PartitionSearcher searcher3;
		 private PartitionSearcher _searcher3;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void partitionSearchersAreClosed() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PartitionSearchersAreClosed()
		 {
			  PartitionedUniquenessVerifier verifier = CreatePartitionedVerifier();

			  verifier.Dispose();

			  verify( _searcher1 ).close();
			  verify( _searcher2 ).close();
			  verify( _searcher3 ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void verifyPropertyUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void VerifyPropertyUpdates()
		 {
			  PartitionedUniquenessVerifier verifier = CreatePartitionedVerifier();
			  NodePropertyAccessor nodePropertyAccessor = mock( typeof( NodePropertyAccessor ) );

			  verifier.Verify( nodePropertyAccessor, new int[]{ 42 }, valueTupleList( "a", "b" ) );

			  VerifySearchInvocations( _searcher1, "a", "b" );
			  VerifySearchInvocations( _searcher2, "a", "b" );
			  VerifySearchInvocations( _searcher3, "a", "b" );
		 }

		 private PartitionedUniquenessVerifier CreatePartitionedVerifier()
		 {
			  return new PartitionedUniquenessVerifier( Searchers );
		 }

		 private IList<PartitionSearcher> Searchers
		 {
			 get
			 {
				  return Arrays.asList( _searcher1, _searcher2, _searcher3 );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void verifySearchInvocations(org.neo4j.kernel.api.impl.index.partition.PartitionSearcher searcher, Object... values) throws java.io.IOException
		 private static void VerifySearchInvocations( PartitionSearcher searcher, params object[] values )
		 {
			  foreach ( object value in values )
			  {
					verify( searcher.IndexSearcher ).search( eq( LuceneDocumentStructure.newSeekQuery( Values.of( value ) ) ), any( typeof( DuplicateCheckingCollector ) ) );
			  }
		 }
	}

}