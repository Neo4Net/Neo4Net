using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.GraphDb
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TestName = org.junit.rules.TestName;


	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.map;

	public abstract class IndexingStringQueryAcceptanceTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static Neo4Net.test.rule.ImpermanentDatabaseRule dbRule = new Neo4Net.test.rule.ImpermanentDatabaseRule();
		 public static ImpermanentDatabaseRule DbRule = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.TestName testName = new org.junit.rules.TestName();
		 public readonly TestName TestName = new TestName();

		 private readonly string _template;
		 private readonly string[] _matching;
		 private readonly string[] _nonMatching;
		 private readonly StringSearchMode _searchMode;
		 private readonly bool _withIndex;

		 private Label _label;
		 private string _key = "name";
		 private IGraphDatabaseService _db;

		 internal IndexingStringQueryAcceptanceTestBase( string template, string[] matching, string[] nonMatching, StringSearchMode searchMode, bool withIndex )
		 {
			  this._template = template;
			  this._matching = matching;
			  this._nonMatching = nonMatching;
			  this._searchMode = searchMode;
			  this._withIndex = withIndex;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _label = Label.label( "LABEL1-" + TestName.MethodName );
			  _db = DbRule.GraphDatabaseAPI;
			  if ( _withIndex )
			  {
					using ( Neo4Net.GraphDb.Transaction tx = _db.beginTx() )
					{
						 _db.schema().indexFor(_label).on(_key).create();
						 tx.Success();
					}

					using ( Neo4Net.GraphDb.Transaction tx = _db.beginTx() )
					{
						 _db.schema().awaitIndexesOnline(5, TimeUnit.MINUTES);
						 tx.Success();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportIndexSeek()
		 public virtual void ShouldSupportIndexSeek()
		 {
			  // GIVEN
			  CreateNodes( _db, _label, _nonMatching );
			  LongSet expected = CreateNodes( _db, _label, _matching );

			  // WHEN
			  MutableLongSet found = new LongHashSet();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CollectNodes( found, _db.findNodes( _label, _key, _template, _searchMode ) );
			  }

			  // THEN
			  assertThat( found, equalTo( expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeNodesCreatedInSameTxInIndexSeek()
		 public virtual void ShouldIncludeNodesCreatedInSameTxInIndexSeek()
		 {
			  // GIVEN
			  CreateNodes( _db, _label, _nonMatching[0], _nonMatching[1] );
			  MutableLongSet expected = CreateNodes( _db, _label, _matching[0], _matching[1] );
			  // WHEN
			  MutableLongSet found = new LongHashSet();
			  using ( Transaction tx = _db.beginTx() )
			  {
					expected.add( CreateNode( _db, map( _key, _matching[2] ), _label ).Id );
					CreateNode( _db, map( _key, _nonMatching[2] ), _label );

					CollectNodes( found, _db.findNodes( _label, _key, _template, _searchMode ) );
			  }
			  // THEN
			  assertThat( found, equalTo( expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotIncludeNodesDeletedInSameTxInIndexSeek()
		 public virtual void ShouldNotIncludeNodesDeletedInSameTxInIndexSeek()
		 {
			  // GIVEN
			  CreateNodes( _db, _label, _nonMatching[0] );
			  LongSet toDelete = CreateNodes( _db, _label, _matching[0], _nonMatching[1], _matching[1], _nonMatching[2] );
			  MutableLongSet expected = CreateNodes( _db, _label, _matching[2] );
			  // WHEN
			  MutableLongSet found = new LongHashSet();
			  using ( Transaction tx = _db.beginTx() )
			  {
					LongIterator deleting = toDelete.longIterator();
					while ( deleting.hasNext() )
					{
						 long id = deleting.next();
						 _db.getNodeById( id ).delete();
						 expected.remove( id );
					}

					CollectNodes( found, _db.findNodes( _label, _key, _template, _searchMode ) );
			  }
			  // THEN
			  assertThat( found, equalTo( expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConsiderNodesChangedInSameTxInIndexSeek()
		 public virtual void ShouldConsiderNodesChangedInSameTxInIndexSeek()
		 {
			  // GIVEN
			  CreateNodes( _db, _label, _nonMatching[0] );
			  LongSet toChangeToMatch = CreateNodes( _db, _label, _nonMatching[1] );
			  MutableLongSet toChangeToNotMatch = CreateNodes( _db, _label, _matching[0] );
			  MutableLongSet expected = CreateNodes( _db, _label, _matching[1] );
			  // WHEN
			  MutableLongSet found = new LongHashSet();
			  using ( Transaction tx = _db.beginTx() )
			  {
					LongIterator toMatching = toChangeToMatch.longIterator();
					while ( toMatching.hasNext() )
					{
						 long id = toMatching.next();
						 _db.getNodeById( id ).setProperty( _key, _matching[2] );
						 expected.add( id );
					}
					LongIterator toNotMatching = toChangeToNotMatch.longIterator();
					while ( toNotMatching.hasNext() )
					{
						 long id = toNotMatching.next();
						 _db.getNodeById( id ).setProperty( _key, _nonMatching[2] );
						 expected.remove( id );
					}

					CollectNodes( found, _db.findNodes( _label, _key, _template, _searchMode ) );
			  }
			  // THEN
			  assertThat( found, equalTo( expected ) );
		 }

		 public abstract class EXACT : IndexingStringQueryAcceptanceTestBase
		 {
			  internal static string[] Matching = new string[] { "Johan", "Johan", "Johan" };
			  internal static string[] NonMatching = new string[] { "Johanna", "Olivia", "InteJohan" };

			  internal EXACT( bool withIndex ) : base( "Johan", Matching, NonMatching, StringSearchMode.Exact, withIndex )
			  {
			  }
		 }

		 public class EXACT_WITH_INDEX : EXACT
		 {
			  public EXACT_WITH_INDEX() : base(true)
			  {
			  }
		 }

		 public class EXACT_WITHOUT_INDEX : EXACT
		 {
			  public EXACT_WITHOUT_INDEX() : base(false)
			  {
			  }
		 }

		 public abstract class PREFIX : IndexingStringQueryAcceptanceTestBase
		 {
			  internal static string[] Matching = new string[] { "Olivia", "Olivia2", "OliviaYtterbrink" };
			  internal static string[] NonMatching = new string[] { "Johan", "olivia", "InteOlivia" };

			  internal PREFIX( bool withIndex ) : base( "Olivia", Matching, NonMatching, StringSearchMode.Prefix, withIndex )
			  {
			  }
		 }

		 public class PREFIX_WITH_INDEX : PREFIX
		 {
			  public PREFIX_WITH_INDEX() : base(true)
			  {
			  }
		 }

		 public class PREFIX_WITHOUT_INDEX : PREFIX
		 {
			  public PREFIX_WITHOUT_INDEX() : base(false)
			  {
			  }
		 }

		 public abstract class SUFFIX : IndexingStringQueryAcceptanceTestBase
		 {
			  internal static string[] Matching = new string[] { "Jansson", "Hansson", "Svensson" };
			  internal static string[] NonMatching = new string[] { "Taverner", "Svensson-Averbuch", "Taylor" };

			  internal SUFFIX( bool withIndex ) : base( "sson", Matching, NonMatching, StringSearchMode.Suffix, withIndex )
			  {
			  }
		 }

		 public class SUFFIX_WITH_INDEX : SUFFIX
		 {
			  public SUFFIX_WITH_INDEX() : base(true)
			  {
			  }
		 }

		 public class SUFFIX_WITHOUT_INDEX : SUFFIX
		 {
			  public SUFFIX_WITHOUT_INDEX() : base(false)
			  {
			  }
		 }

		 public abstract class CONTAINS : IndexingStringQueryAcceptanceTestBase
		 {
			  internal static string[] Matching = new string[] { "good", "fool", "fooooood" };
			  internal static string[] NonMatching = new string[] { "evil", "genius", "hungry" };

			  public CONTAINS( bool withIndex ) : base( "oo", Matching, NonMatching, StringSearchMode.Contains, withIndex )
			  {
			  }
		 }

		 public class CONTAINS_WITH_INDEX : CONTAINS
		 {
			  public CONTAINS_WITH_INDEX() : base(true)
			  {
			  }
		 }

		 public class CONTAINS_WITHOUT_INDEX : CONTAINS
		 {
			  public CONTAINS_WITHOUT_INDEX() : base(false)
			  {
			  }
		 }

		 private MutableLongSet CreateNodes( IGraphDatabaseService db, Label label, params string[] propertyValues )
		 {
			  MutableLongSet expected = new LongHashSet();
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( string value in propertyValues )
					{
						 expected.add( CreateNode( db, map( _key, value ), label ).Id );
					}
					tx.Success();
			  }
			  return expected;
		 }

		 private void CollectNodes( MutableLongSet bucket, IResourceIterator<Node> toCollect )
		 {
			  while ( toCollect.MoveNext() )
			  {
					bucket.add( toCollect.Current.Id );
			  }
		 }

		 private Node CreateNode( IGraphDatabaseService beansAPI, IDictionary<string, object> properties, params Label[] labels )
		 {
			  using ( Transaction tx = beansAPI.BeginTx() )
			  {
					Node node = beansAPI.CreateNode( labels );
					foreach ( KeyValuePair<string, object> property in properties.SetOfKeyValuePairs() )
					{
						 node.SetProperty( property.Key, property.Value );
					}
					tx.Success();
					return node;
			  }
		 }
	}

}