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
namespace Org.Neo4j.Kernel.Impl.Api
{
	using Assume = org.junit.Assume;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using NodeValueIndexCursor = Org.Neo4j.@internal.Kernel.Api.NodeValueIndexCursor;
	using TokenRead = Org.Neo4j.@internal.Kernel.Api.TokenRead;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using IndexNotApplicableKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotApplicableKernelException;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using TestIndexDescriptorFactory = Org.Neo4j.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Org.Neo4j.Test.rule.EmbeddedDatabaseRule;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;
	using TextValue = Org.Neo4j.Values.Storable.TextValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.collection.IsIterableContainingInAnyOrder.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	/// <summary>
	/// Does test having multiple iterators open on the same index
	/// <ul>
	/// <li>Exhaust variations:</li>
	/// <ul>
	/// <li>Exhaust iterators one by one</li>
	/// <li>Nesting</li>
	/// <li>Interleaved</li>
	/// </ul>
	/// <li>Happy case for schema index iterators on static db for:</li>
	/// <ul>
	/// <li>Single property number index</li>
	/// <li>Single property string index</li>
	/// <li>Composite property number index</li>
	/// <li>Composite property string index</li>
	/// </ul>
	/// <li>For index queries:</li>
	/// <ul>
	/// <li><seealso cref="IndexQuery.exists(int)"/></li>
	/// <li><seealso cref="IndexQuery.exact(int, object)"/></li>
	/// <li><seealso cref="IndexQuery.range(int, Number, bool, Number, bool)"/></li>
	/// <li><seealso cref="IndexQuery.range(int, string, bool, string, bool)"/></li>
	/// </ul>
	/// </ul>
	/// Does NOT test
	/// <ul>
	/// <li>Single property unique number index</li>
	/// <li>Single property unique string index</li>
	/// <li>Composite property mixed index</li>
	/// <li><seealso cref="IndexQuery.stringSuffix(int, TextValue)"/></li>
	/// <li><seealso cref="IndexQuery.stringSuffix(int, TextValue)"/></li>
	/// <li><seealso cref="IndexQuery.stringContains(int, TextValue)"/></li>
	/// <li>Composite property node key index (due to it being enterprise feature)</li>
	/// <li>Label index iterators</li>
	/// <li>Concurrency</li>
	/// <li>Locking</li>
	/// <li>Cluster</li>
	/// <li>Index creation</li>
	/// </ul>
	/// Code navigation:
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class MultipleOpenCursorsTest
	public class MultipleOpenCursorsTest
	{
		private bool InstanceFieldsInitialized = false;

		public MultipleOpenCursorsTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _rnd ).around( _db );
		}

		 private interface IndexCoordinatorFactory
		 {
			  IndexCoordinator Create( Label indexLabel, string numberProp1, string numberProp2, string stringProp1, string stringProp2 );
		 }

		 private readonly DatabaseRule _db = new EmbeddedDatabaseRule();

		 private static readonly RandomRule _rnd = new RandomRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(rnd).around(db);
		 public RuleChain RuleChain;

		 private static readonly Label _indexLabel = Label.label( "IndexLabel" );

		 private const string NUMBER_PROP1 = "numberProp1";
		 private const string NUMBER_PROP2 = "numberProp2";
		 private const string STRING_PROP1 = "stringProp1";
		 private const string STRING_PROP2 = "stringProp2";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<Object[]> params()
		 public static ICollection<object[]> Params()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Arrays.asList( new object[]{ "Single number non unique", ( IndexCoordinatorFactory ) NumberIndexCoordinator::new }, new object[]{ "Single string non unique", ( IndexCoordinatorFactory ) StringIndexCoordinator::new }, new object[]{ "Composite number non unique", ( IndexCoordinatorFactory ) NumberCompositeIndexCoordinator::new }, new object[]{ "Composite string non unique", ( IndexCoordinatorFactory ) StringCompositeIndexCoordinator::new } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public String name;
		 public string Name;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public IndexCoordinatorFactory indexCoordinatorFactory;
		 public IndexCoordinatorFactory IndexCoordinatorFactory;

		 private IndexCoordinator _indexCoordinator;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupDb()
		 public virtual void SetupDb()
		 {
			  _indexCoordinator = IndexCoordinatorFactory.create( _indexLabel, NUMBER_PROP1, NUMBER_PROP2, STRING_PROP1, STRING_PROP2 );
			  _indexCoordinator.init( _db );
			  _indexCoordinator.createIndex( _db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleCursorsNotNestedExists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MultipleCursorsNotNestedExists()
		 {

			  using ( Transaction tx = _db.beginTx() )
			  {
					KernelTransaction ktx = _db.transaction();
					// when
					using ( NodeValueIndexCursor cursor1 = _indexCoordinator.queryExists( ktx ), NodeValueIndexCursor cursor2 = _indexCoordinator.queryExists( ktx ) )
					{
						 IList<long> actual1 = new IList<long> { cursor1 };
						 IList<long> actual2 = new IList<long> { cursor2 };

						 // then
						 _indexCoordinator.assertExistsResult( actual1 );
						 _indexCoordinator.assertExistsResult( actual2 );
					}

					tx.Success();
			  }
		 }

		 private IList<long> AsList( NodeValueIndexCursor cursor )
		 {
			  IList<long> list = new List<long>();
			  while ( cursor.Next() )
			  {
					list.Add( cursor.NodeReference() );
			  }
			  return list;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleCursorsNotNestedExact() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MultipleCursorsNotNestedExact()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					// when
					KernelTransaction ktx = _db.transaction();
					using ( NodeValueIndexCursor cursor1 = _indexCoordinator.queryExact( ktx ), NodeValueIndexCursor cursor2 = _indexCoordinator.queryExact( ktx ) )
					{
						 IList<long> actual1 = new IList<long> { cursor1 };
						 IList<long> actual2 = new IList<long> { cursor2 };

						 // then
						 _indexCoordinator.assertExactResult( actual1 );
						 _indexCoordinator.assertExactResult( actual2 );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleIteratorsNotNestedRange() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MultipleIteratorsNotNestedRange()
		 {
			  Assume.assumeTrue( _indexCoordinator.supportRangeQuery() );
			  using ( Transaction tx = _db.beginTx() )
			  {
					// when
					KernelTransaction ktx = _db.transaction();
					using ( NodeValueIndexCursor cursor1 = _indexCoordinator.queryRange( ktx ), NodeValueIndexCursor cursor2 = _indexCoordinator.queryRange( ktx ) )
					{
						 IList<long> actual1 = new IList<long> { cursor1 };
						 IList<long> actual2 = new IList<long> { cursor2 };

						 // then
						 _indexCoordinator.assertRangeResult( actual1 );
						 _indexCoordinator.assertRangeResult( actual2 );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleIteratorsNestedInnerNewExists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MultipleIteratorsNestedInnerNewExists()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					// when
					KernelTransaction ktx = _db.transaction();
					using ( NodeValueIndexCursor cursor1 = _indexCoordinator.queryExists( ktx ) )
					{
						 IList<long> actual1 = new List<long>();
						 while ( cursor1.Next() )
						 {
							  actual1.Add( cursor1.NodeReference() );

							  using ( NodeValueIndexCursor cursor2 = _indexCoordinator.queryExists( ktx ) )
							  {
									IList<long> actual2 = new IList<long> { cursor2 };
									_indexCoordinator.assertExistsResult( actual2 );
							  }
						 }
						 // then
						 _indexCoordinator.assertExistsResult( actual1 );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleIteratorsNestedInnerNewExact() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MultipleIteratorsNestedInnerNewExact()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					// when
					KernelTransaction ktx = _db.transaction();
					using ( NodeValueIndexCursor cursor1 = _indexCoordinator.queryExact( ktx ) )
					{
						 IList<long> actual1 = new List<long>();
						 while ( cursor1.Next() )
						 {
							  actual1.Add( cursor1.NodeReference() );
							  using ( NodeValueIndexCursor cursor2 = _indexCoordinator.queryExact( ktx ) )
							  {
									IList<long> actual2 = new IList<long> { cursor2 };
									_indexCoordinator.assertExactResult( actual2 );
							  }
						 }
						 // then
						 _indexCoordinator.assertExactResult( actual1 );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleIteratorsNestedInnerNewRange() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MultipleIteratorsNestedInnerNewRange()
		 {
			  Assume.assumeTrue( _indexCoordinator.supportRangeQuery() );
			  using ( Transaction tx = _db.beginTx() )
			  {
					// when
					KernelTransaction ktx = _db.transaction();
					using ( NodeValueIndexCursor cursor1 = _indexCoordinator.queryRange( ktx ) )
					{
						 IList<long> actual1 = new List<long>();
						 while ( cursor1.Next() )
						 {
							  actual1.Add( cursor1.NodeReference() );
							  using ( NodeValueIndexCursor cursor2 = _indexCoordinator.queryRange( ktx ) )
							  {
									IList<long> actual2 = new IList<long> { cursor2 };
									_indexCoordinator.assertRangeResult( actual2 );
							  }
						 }
						 // then
						 _indexCoordinator.assertRangeResult( actual1 );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleIteratorsNestedInterleavedExists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MultipleIteratorsNestedInterleavedExists()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					// when
					KernelTransaction ktx = _db.transaction();
					using ( NodeValueIndexCursor cursor1 = _indexCoordinator.queryExists( ktx ) )
					{
						 IList<long> actual1 = new List<long>();
						 using ( NodeValueIndexCursor cursor2 = _indexCoordinator.queryExists( ktx ) )
						 {
							  IList<long> actual2 = new List<long>();

							  // Interleave
							  ExhaustInterleaved( cursor1, actual1, cursor2, actual2 );

							  // then
							  _indexCoordinator.assertExistsResult( actual1 );
							  _indexCoordinator.assertExistsResult( actual2 );
						 }
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleIteratorsNestedInterleavedExact() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MultipleIteratorsNestedInterleavedExact()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					// when
					KernelTransaction ktx = _db.transaction();
					using ( NodeValueIndexCursor cursor1 = _indexCoordinator.queryExact( ktx ) )
					{
						 IList<long> actual1 = new List<long>();
						 using ( NodeValueIndexCursor cursor2 = _indexCoordinator.queryExact( ktx ) )
						 {
							  IList<long> actual2 = new List<long>();

							  // Interleave
							  ExhaustInterleaved( cursor1, actual1, cursor2, actual2 );

							  // then
							  _indexCoordinator.assertExactResult( actual1 );
							  _indexCoordinator.assertExactResult( actual2 );
						 }
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleIteratorsNestedInterleavedRange() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MultipleIteratorsNestedInterleavedRange()
		 {
			  Assume.assumeTrue( _indexCoordinator.supportRangeQuery() );
			  using ( Transaction tx = _db.beginTx() )
			  {
					// when
					KernelTransaction ktx = _db.transaction();
					using ( NodeValueIndexCursor cursor1 = _indexCoordinator.queryRange( ktx ), NodeValueIndexCursor cursor2 = _indexCoordinator.queryRange( ktx ) )
					{
						 IList<long> actual1 = new List<long>();

						 IList<long> actual2 = new List<long>();

						 // Interleave
						 ExhaustInterleaved( cursor1, actual1, cursor2, actual2 );

						 // then
						 _indexCoordinator.assertRangeResult( actual1 );
						 _indexCoordinator.assertRangeResult( actual2 );
					}
					tx.Success();
			  }
		 }

		 private void ExhaustInterleaved( NodeValueIndexCursor source1, IList<long> target1, NodeValueIndexCursor source2, IList<long> target2 )
		 {
			  bool source1HasNext = true;
			  bool source2HasNext = true;
			  while ( source1HasNext && source2HasNext )
			  {
					if ( _rnd.nextBoolean() )
					{
						 source1HasNext = source1.Next();
						 if ( source1HasNext )
						 {
							  target1.Add( source1.NodeReference() );
						 }
					}
					else
					{
						 source2HasNext = source2.Next();
						 if ( source2HasNext )
						 {
							  target2.Add( source2.NodeReference() );
						 }
					}
			  }

			  // Empty the rest
			  while ( source1.Next() )
			  {
					target1.Add( source1.NodeReference() );
			  }
			  while ( source2.Next() )
			  {
					target2.Add( source2.NodeReference() );
			  }
		 }

		 private class StringCompositeIndexCoordinator : IndexCoordinator
		 {
			  internal StringCompositeIndexCoordinator( Label indexLabel, string numberProp1, string numberProp2, string stringProp1, string stringProp2 ) : base( indexLabel, numberProp1, numberProp2, stringProp1, stringProp2 )
			  {
			  }

			  protected internal override IndexDescriptor ExtractIndexDescriptor()
			  {
					return TestIndexDescriptorFactory.forLabel( IndexedLabelId, StringPropId1, StringPropId2 );
			  }

			  internal override bool SupportRangeQuery()
			  {
					return false;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: NodeValueIndexCursor queryRange(org.neo4j.kernel.api.KernelTransaction ktx) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
			  internal override NodeValueIndexCursor QueryRange( KernelTransaction ktx )
			  {
					throw new System.NotSupportedException();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: NodeValueIndexCursor queryExists(org.neo4j.kernel.api.KernelTransaction ktx) throws org.neo4j.internal.kernel.api.exceptions.KernelException
			  internal override NodeValueIndexCursor QueryExists( KernelTransaction ktx )
			  {
					return IndexQuery( ktx, IndexDescriptor, IndexQuery.exists( StringPropId1 ), IndexQuery.exists( StringPropId2 ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: NodeValueIndexCursor queryExact(org.neo4j.kernel.api.KernelTransaction ktx) throws org.neo4j.internal.kernel.api.exceptions.KernelException
			  internal override NodeValueIndexCursor QueryExact( KernelTransaction ktx )
			  {
					return IndexQuery( ktx, IndexDescriptor, IndexQuery.exact( StringPropId1, StringProp1Values[0] ), IndexQuery.exact( StringPropId2, StringProp2Values[0] ) );
			  }

			  internal override void AssertRangeResult( IList<long> result )
			  {
					throw new System.NotSupportedException();
			  }

			  internal override void AssertExactResult( IList<long> actual )
			  {
					IList<long> expected = new List<long>();
					expected.Add( 0L );
					AssertSameContent( actual, expected );
			  }

			  internal override void DoCreateIndex( DatabaseRule db )
			  {
					Db.schema().indexFor(IndexLabel).on(StringProp1).on(StringProp2).create();
			  }
		 }

		 private class NumberCompositeIndexCoordinator : IndexCoordinator
		 {
			  internal NumberCompositeIndexCoordinator( Label indexLabel, string numberProp1, string numberProp2, string stringProp1, string stringProp2 ) : base( indexLabel, numberProp1, numberProp2, stringProp1, stringProp2 )
			  {
			  }

			  protected internal override IndexDescriptor ExtractIndexDescriptor()
			  {
					return TestIndexDescriptorFactory.forLabel( IndexedLabelId, NumberPropId1, NumberPropId2 );
			  }

			  internal override bool SupportRangeQuery()
			  {
					return false;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: NodeValueIndexCursor queryRange(org.neo4j.kernel.api.KernelTransaction ktx) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
			  internal override NodeValueIndexCursor QueryRange( KernelTransaction ktx )
			  {
					throw new System.NotSupportedException();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: NodeValueIndexCursor queryExists(org.neo4j.kernel.api.KernelTransaction ktx) throws org.neo4j.internal.kernel.api.exceptions.KernelException
			  internal override NodeValueIndexCursor QueryExists( KernelTransaction ktx )
			  {
					return IndexQuery( ktx, IndexDescriptor, IndexQuery.exists( NumberPropId1 ), IndexQuery.exists( NumberPropId2 ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: NodeValueIndexCursor queryExact(org.neo4j.kernel.api.KernelTransaction ktx) throws org.neo4j.internal.kernel.api.exceptions.KernelException
			  internal override NodeValueIndexCursor QueryExact( KernelTransaction ktx )
			  {
					return IndexQuery( ktx, IndexDescriptor, IndexQuery.exact( NumberPropId1, NumberProp1Values[0] ), IndexQuery.exact( NumberPropId2, NumberProp2Values[0] ) );
			  }

			  internal override void AssertRangeResult( IList<long> actual )
			  {
					throw new System.NotSupportedException();
			  }

			  internal override void AssertExactResult( IList<long> actual )
			  {
					IList<long> expected = new List<long>();
					expected.Add( 0L );
					AssertSameContent( actual, expected );
			  }

			  internal override void DoCreateIndex( DatabaseRule db )
			  {
					Db.schema().indexFor(IndexLabel).on(NumberProp1).on(NumberProp2).create();
			  }
		 }

		 private class StringIndexCoordinator : IndexCoordinator
		 {
			  internal StringIndexCoordinator( Label indexLabel, string numberProp1, string numberProp2, string stringProp1, string stringProp2 ) : base( indexLabel, numberProp1, numberProp2, stringProp1, stringProp2 )
			  {
			  }

			  protected internal override IndexDescriptor ExtractIndexDescriptor()
			  {
					return TestIndexDescriptorFactory.forLabel( IndexedLabelId, StringPropId1 );
			  }

			  internal override bool SupportRangeQuery()
			  {
					return true;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: NodeValueIndexCursor queryRange(org.neo4j.kernel.api.KernelTransaction ktx) throws org.neo4j.internal.kernel.api.exceptions.KernelException
			  internal override NodeValueIndexCursor QueryRange( KernelTransaction ktx )
			  {
					// query for half the range
					return IndexQuery( ktx, IndexDescriptor, IndexQuery.range( NumberPropId1, StringProp1Values[0], true, StringProp1Values[NumberOfNodes / 2], false ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: NodeValueIndexCursor queryExists(org.neo4j.kernel.api.KernelTransaction ktx) throws org.neo4j.internal.kernel.api.exceptions.KernelException
			  internal override NodeValueIndexCursor QueryExists( KernelTransaction ktx )
			  {
					return IndexQuery( ktx, IndexDescriptor, IndexQuery.exists( StringPropId1 ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: NodeValueIndexCursor queryExact(org.neo4j.kernel.api.KernelTransaction ktx) throws org.neo4j.internal.kernel.api.exceptions.KernelException
			  internal override NodeValueIndexCursor QueryExact( KernelTransaction ktx )
			  {
					return IndexQuery( ktx, IndexDescriptor, IndexQuery.exact( StringPropId1, StringProp1Values[0] ) );
			  }

			  internal override void AssertRangeResult( IList<long> actual )
			  {
					IList<long> expected = new List<long>();
					for ( long i = 0; i < NumberOfNodes / 2; i++ )
					{
						 expected.Add( i );
					}
					AssertSameContent( actual, expected );
			  }

			  internal override void AssertExactResult( IList<long> actual )
			  {
					IList<long> expected = new List<long>();
					expected.Add( 0L );
					AssertSameContent( actual, expected );
			  }

			  internal override void DoCreateIndex( DatabaseRule db )
			  {
					Db.schema().indexFor(IndexLabel).on(StringProp1).create();
			  }
		 }

		 private class NumberIndexCoordinator : IndexCoordinator
		 {
			  internal NumberIndexCoordinator( Label indexLabel, string numberProp1, string numberProp2, string stringProp1, string stringProp2 ) : base( indexLabel, numberProp1, numberProp2, stringProp1, stringProp2 )
			  {
			  }

			  protected internal override IndexDescriptor ExtractIndexDescriptor()
			  {
					return TestIndexDescriptorFactory.forLabel( IndexedLabelId, NumberPropId1 );
			  }

			  internal override bool SupportRangeQuery()
			  {
					return true;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: NodeValueIndexCursor queryRange(org.neo4j.kernel.api.KernelTransaction ktx) throws org.neo4j.internal.kernel.api.exceptions.KernelException
			  internal override NodeValueIndexCursor QueryRange( KernelTransaction ktx )
			  {
					// query for half the range
					return IndexQuery( ktx, IndexDescriptor, IndexQuery.range( NumberPropId1, NumberProp1Values[0], true, NumberProp1Values[NumberOfNodes / 2], false ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: NodeValueIndexCursor queryExists(org.neo4j.kernel.api.KernelTransaction ktx) throws org.neo4j.internal.kernel.api.exceptions.KernelException
			  internal override NodeValueIndexCursor QueryExists( KernelTransaction ktx )
			  {
					return IndexQuery( ktx, IndexDescriptor, IndexQuery.exists( NumberPropId1 ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: NodeValueIndexCursor queryExact(org.neo4j.kernel.api.KernelTransaction ktx) throws org.neo4j.internal.kernel.api.exceptions.KernelException
			  internal override NodeValueIndexCursor QueryExact( KernelTransaction ktx )
			  {
					return IndexQuery( ktx, IndexDescriptor, IndexQuery.exact( NumberPropId1, NumberProp1Values[0] ) );
			  }

			  internal override void AssertRangeResult( IList<long> actual )
			  {
					IList<long> expected = new List<long>();
					for ( long i = 0; i < NumberOfNodes / 2; i++ )
					{
						 expected.Add( i );
					}
					AssertSameContent( actual, expected );
			  }

			  internal override void AssertExactResult( IList<long> actual )
			  {
					IList<long> expected = new List<long>();
					expected.Add( 0L );
					AssertSameContent( actual, expected );
			  }

			  internal override void DoCreateIndex( DatabaseRule db )
			  {
					Db.schema().indexFor(IndexLabel).on(NumberProp1).create();
			  }
		 }

		 private abstract class IndexCoordinator
		 {
			  internal readonly int NumberOfNodes = 100;

			  internal readonly Label IndexLabel;
			  internal readonly string NumberProp1;
			  internal readonly string NumberProp2;
			  internal readonly string StringProp1;
			  internal readonly string StringProp2;

			  internal Number[] NumberProp1Values;
			  internal Number[] NumberProp2Values;
			  internal string[] StringProp1Values;
			  internal string[] StringProp2Values;

			  internal int IndexedLabelId;
			  internal int NumberPropId1;
			  internal int NumberPropId2;
			  internal int StringPropId1;
			  internal int StringPropId2;
			  internal IndexDescriptor IndexDescriptor;

			  internal IndexCoordinator( Label indexLabel, string numberProp1, string numberProp2, string stringProp1, string stringProp2 )
			  {
					this.IndexLabel = indexLabel;
					this.NumberProp1 = numberProp1;
					this.NumberProp2 = numberProp2;
					this.StringProp1 = stringProp1;
					this.StringProp2 = stringProp2;

					this.NumberProp1Values = new Number[NumberOfNodes];
					this.NumberProp2Values = new Number[NumberOfNodes];
					this.StringProp1Values = new string[NumberOfNodes];
					this.StringProp2Values = new string[NumberOfNodes];

					// EXISTING DATA:
					// 100 nodes with properties:
					// numberProp1: 0-99
					// numberProp2: 0-99
					// stringProp1: "string-0"-"string-99"
					// stringProp2: "string-0"-"string-99"
					for ( int i = 0; i < NumberOfNodes; i++ )
					{
						 NumberProp1Values[i] = i;
						 NumberProp2Values[i] = i;
						 StringProp1Values[i] = "string-" + string.Format( "{0:D2}", i );
						 StringProp2Values[i] = "string-" + string.Format( "{0:D2}", i );
					}
			  }

			  internal virtual void Init( DatabaseRule db )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 for ( int i = 0; i < NumberOfNodes; i++ )
						 {
							  Node node = Db.createNode( IndexLabel );
							  node.SetProperty( NumberProp1, NumberProp1Values[i] );
							  node.SetProperty( NumberProp2, NumberProp2Values[i] );
							  node.SetProperty( StringProp1, StringProp1Values[i] );
							  node.SetProperty( StringProp2, StringProp2Values[i] );
						 }
						 tx.Success();
					}

					using ( Transaction tx = Db.beginTx() )
					{
						 TokenRead tokenRead = Db.transaction().tokenRead();
						 IndexedLabelId = tokenRead.NodeLabel( IndexLabel.name() );
						 NumberPropId1 = tokenRead.PropertyKey( NumberProp1 );
						 NumberPropId2 = tokenRead.PropertyKey( NumberProp2 );
						 StringPropId1 = tokenRead.PropertyKey( StringProp1 );
						 StringPropId2 = tokenRead.PropertyKey( StringProp2 );
						 tx.Success();
					}
					IndexDescriptor = ExtractIndexDescriptor();
			  }

			  protected internal abstract IndexDescriptor ExtractIndexDescriptor();

			  internal virtual void CreateIndex( DatabaseRule db )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 DoCreateIndex( db );
						 tx.Success();
					}
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
						 tx.Success();
					}
			  }

			  internal abstract bool SupportRangeQuery();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract org.neo4j.internal.kernel.api.NodeValueIndexCursor queryRange(org.neo4j.kernel.api.KernelTransaction ktx) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
			  internal abstract NodeValueIndexCursor QueryRange( KernelTransaction ktx );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract org.neo4j.internal.kernel.api.NodeValueIndexCursor queryExists(org.neo4j.kernel.api.KernelTransaction ktx) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
			  internal abstract NodeValueIndexCursor QueryExists( KernelTransaction ktx );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract org.neo4j.internal.kernel.api.NodeValueIndexCursor queryExact(org.neo4j.kernel.api.KernelTransaction ktx) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
			  internal abstract NodeValueIndexCursor QueryExact( KernelTransaction ktx );

			  internal abstract void AssertRangeResult( IList<long> result );

			  internal virtual void AssertExistsResult( IList<long> actual )
			  {
					IList<long> expected = new List<long>();
					for ( long i = 0; i < NumberOfNodes; i++ )
					{
						 expected.Add( i );
					}
					AssertSameContent( actual, expected );
			  }

			  internal virtual void AssertSameContent( IList<long> actual, IList<long> expected )
			  {
					assertThat( actual, @is( containsInAnyOrder( expected.ToArray() ) ) );
			  }

			  internal abstract void AssertExactResult( IList<long> result );

			  internal abstract void DoCreateIndex( DatabaseRule db );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.NodeValueIndexCursor indexQuery(org.neo4j.kernel.api.KernelTransaction ktx, org.neo4j.storageengine.api.schema.IndexDescriptor indexDescriptor, org.neo4j.internal.kernel.api.IndexQuery... indexQueries) throws org.neo4j.internal.kernel.api.exceptions.KernelException
			  internal virtual NodeValueIndexCursor IndexQuery( KernelTransaction ktx, IndexDescriptor indexDescriptor, params IndexQuery[] indexQueries )
			  {
					NodeValueIndexCursor cursor = ktx.Cursors().allocateNodeValueIndexCursor();
					ktx.DataRead().nodeIndexSeek(indexDescriptor, cursor, IndexOrder.NONE, false, indexQueries);
					return cursor;
			  }
		 }
	}

}