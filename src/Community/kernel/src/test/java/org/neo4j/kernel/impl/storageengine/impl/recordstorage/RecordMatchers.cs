using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using Functions = org.eclipse.collections.impl.block.factory.Functions;
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;


	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using RecordChangeSet = Neo4Net.Kernel.impl.transaction.state.RecordChangeSet;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.filterType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.records;

	// Hamcrest matchers for store records
	public class RecordMatchers
	{
		 /// <summary>
		 /// Match a RecordChangeSet </summary>
		 public static DiffMatcher<RecordChangeSet> ContainsChanges( params AbstractBaseRecord[] expectedChanges )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: DiffMatcher<Iterable<? extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord>> nodes = containsRecords("nodes", filterType(expectedChanges, org.neo4j.kernel.impl.store.record.NodeRecord.class));
			  DiffMatcher<IEnumerable<AbstractBaseRecord>> nodes = ContainsRecords( "nodes", filterType( expectedChanges, typeof( NodeRecord ) ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: DiffMatcher<Iterable<? extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord>> rels = containsRecords("relationships", filterType(expectedChanges, org.neo4j.kernel.impl.store.record.RelationshipRecord.class));
			  DiffMatcher<IEnumerable<AbstractBaseRecord>> rels = ContainsRecords( "relationships", filterType( expectedChanges, typeof( RelationshipRecord ) ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: DiffMatcher<Iterable<? extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord>> groups = containsRecords("relationship groups", filterType(expectedChanges, org.neo4j.kernel.impl.store.record.RelationshipGroupRecord.class));
			  DiffMatcher<IEnumerable<AbstractBaseRecord>> groups = ContainsRecords( "relationship groups", filterType( expectedChanges, typeof( RelationshipGroupRecord ) ) );

			  return new DiffMatcherAnonymousInnerClass( expectedChanges, nodes, rels, groups );
		 }

		 private class DiffMatcherAnonymousInnerClass : DiffMatcher<RecordChangeSet>
		 {
			 private AbstractBaseRecord[] _expectedChanges;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.kernel.impl.storageengine.impl.recordstorage.RecordMatchers.DiffMatcher<System.Collections.Generic.IEnumerable<JavaToDotNetGenericWildcard extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord>> nodes;
			 private Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordMatchers.DiffMatcher<IEnumerable<AbstractBaseRecord>> _nodes;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.kernel.impl.storageengine.impl.recordstorage.RecordMatchers.DiffMatcher<System.Collections.Generic.IEnumerable<JavaToDotNetGenericWildcard extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord>> rels;
			 private Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordMatchers.DiffMatcher<IEnumerable<AbstractBaseRecord>> _rels;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.kernel.impl.storageengine.impl.recordstorage.RecordMatchers.DiffMatcher<System.Collections.Generic.IEnumerable<JavaToDotNetGenericWildcard extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord>> groups;
			 private Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordMatchers.DiffMatcher<IEnumerable<AbstractBaseRecord>> _groups;

			 public DiffMatcherAnonymousInnerClass<T1, T2, T3>( AbstractBaseRecord[] expectedChanges, Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordMatchers.DiffMatcher<T1> nodes, Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordMatchers.DiffMatcher<T2> rels, Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordMatchers.DiffMatcher<T3> groups ) where T1 : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord where T2 : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord where T3 : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
			 {
				 this._expectedChanges = expectedChanges;
				 this._nodes = nodes;
				 this._rels = rels;
				 this._groups = groups;
			 }

			 internal string diff( RecordChangeSet actual )
			 {
				  string diff;

				  diff = _nodes.diff( records( actual.NodeRecords.changes() ) );
				  if ( !string.ReferenceEquals( diff, null ) )
				  {
						return diff;
				  }

				  diff = _rels.diff( records( actual.RelRecords.changes() ) );
				  if ( !string.ReferenceEquals( diff, null ) )
				  {
						return diff;
				  }

				  diff = _groups.diff( records( actual.RelGroupRecords.changes() ) );
				  if ( !string.ReferenceEquals( diff, null ) )
				  {
						return diff;
				  }

				  return null;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendValueList( "[", ",", "]", _expectedChanges );
			 }
		 }

		 // Build a contains matcher that matches all records of a single given type
		 // NOTE: This is a bit brittle, if you like it'd be easy to make it a general purpose
		 // list-of-records-of-any-type matcher. As-is, if you use it to match mixed-type records,
		 // behavior is undefined.
		 // NOTE: This nests diff functions for individual records; if you want a matcher for
		 // a single record, just refactor those out and have this delegate to them, see how
		 // the containsChanges delegates here for an example.
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static DiffMatcher<Iterable<? extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord>> containsRecords(String recordPlural, java.util.stream.Stream<? extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord> expected)
		 public static DiffMatcher<IEnumerable<AbstractBaseRecord>> ContainsRecords<T1>( string recordPlural, Stream<T1> expected ) where T1 : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IDictionary<long, AbstractBaseRecord> expectedById = expected.collect( Collectors.toMap( AbstractBaseRecord::getId, Functions.identity() ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return new DiffMatcher<Iterable<? extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord>>()
			  return new DiffMatcherAnonymousInnerClass2( recordPlural, expected, expectedById );
		 }

		 private class DiffMatcherAnonymousInnerClass2 : DiffMatcher<IEnumerable<JavaToDotNetGenericWildcard extends AbstractBaseRecord>>
		 {
			 private string _recordPlural;
			 private Stream<T1> _expected;
			 private IDictionary<long, AbstractBaseRecord> _expectedById;

			 public DiffMatcherAnonymousInnerClass2( string recordPlural, Stream<T1> expected, IDictionary<long, AbstractBaseRecord> expectedById )
			 {
				 this._recordPlural = recordPlural;
				 this._expected = expected;
				 this._expectedById = expectedById;
			 }

			 internal string diff<T1>( IEnumerable<T1> actual ) where T1 : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
			 {
				  ISet<long> seen = new HashSet<long>( _expectedById.Keys );
				  foreach ( AbstractBaseRecord record in actual )
				  {
						seen.remove( record.Id );
						if ( !_expectedById.ContainsKey( record.Id ) )
						{
							 return string.Format( "This record was not expected: {0}", record );
						}

						string diff = diff( _expectedById[record.Id], record );
						if ( !string.ReferenceEquals( diff, null ) )
						{
							 return diff;
						}
				  }

				  return null;
			 }

			 private string diff( AbstractBaseRecord expected, AbstractBaseRecord actual )
			 {
				  if ( expected is NodeRecord )
				  {
						return diff( ( NodeRecord ) expected, ( NodeRecord ) actual );
				  }
				  if ( expected is RelationshipRecord )
				  {
						return diff( ( RelationshipRecord ) expected, ( RelationshipRecord ) actual );
				  }
				  if ( expected is RelationshipGroupRecord )
				  {
						return diff( ( RelationshipGroupRecord ) expected, ( RelationshipGroupRecord ) actual );
				  }
				  throw new System.NotSupportedException( string.Format( "No diff implementation (just add one, its easy) for: {0}", expected ) );
			 }

			 private string diff( NodeRecord expected, NodeRecord actual )
			 {
				  if ( actual.Id == expected.Id && actual.NextRel == expected.NextRel && actual.LabelField == expected.LabelField && actual.NextProp == expected.NextProp && actual.Dense == expected.Dense && actual.Light == expected.Light )
				  {
						return null;
				  }
				  return describeDiff( expected.ToString(), actual.ToString() );
			 }

			 private string diff( RelationshipGroupRecord expected, RelationshipGroupRecord actual )
			 {
				  if ( actual.Id == expected.Id && actual.Type == expected.Type && actual.Next == expected.Next && actual.FirstOut == expected.FirstOut && actual.FirstIn == expected.FirstIn && actual.FirstLoop == expected.FirstLoop && actual.OwningNode == expected.OwningNode )
				  {
						return null;
				  }
				  return describeDiff( expected.ToString(), actual.ToString() );
			 }

			 private string diff( RelationshipRecord expected, RelationshipRecord actual )
			 {
				  if ( actual.Id == expected.Id && actual.FirstNode == expected.FirstNode && actual.SecondNode == expected.SecondNode && actual.Type == expected.Type && actual.FirstPrevRel == expected.FirstPrevRel && actual.FirstNextRel == expected.FirstNextRel && actual.SecondPrevRel == expected.SecondPrevRel && actual.SecondNextRel == expected.SecondNextRel && actual.FirstInFirstChain == expected.FirstInFirstChain && actual.FirstInSecondChain == expected.FirstInSecondChain )
				  {
						return null;
				  }
				  return describeDiff( expected.ToString(), actual.ToString() );
			 }

			 private string describeDiff( string expected, string actual )
			 {
				  StringBuilder arrow = new StringBuilder();
				  char[] expectedChars = expected.ToCharArray();
				  char[] actualChars = actual.ToCharArray();
				  for ( int i = 0; i < Math.Min( expectedChars.Length, actualChars.Length ); i++ )
				  {
						if ( expectedChars[i] != actualChars[i] )
						{
							 break;
						}
						arrow.Append( "-" );
				  }
				  return string.Format( "Record fields don't match.\n" + "Expected: {0}\n" + "Actual:   {1}\n" + "          {2}", expected, actual, arrow.Append( "^" ).ToString() );
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendValueList( string.Format( "{0} matching:\n  ", _recordPlural ), "\n  ", "", _expectedById.Values );
			 }
		 }

		 // Matcher where you implement a common "diff" describer, which fails if the
		 // diff is non-null. Benefit here being that you don't have to duplicate the
		 // match logic in the mismatch description; you write one function to find difference
		 // and get both match and describeMismatch implemented for you.
		 public abstract class DiffMatcher<T> : TypeSafeMatcher<T>
		 {
			  internal abstract string Diff( T item );

			  protected internal override bool MatchesSafely( T item )
			  {
					return string.ReferenceEquals( Diff( item ), null );
			  }

			  protected internal override void DescribeMismatchSafely( T item, Description mismatchDescription )
			  {
					mismatchDescription.appendText( Diff( item ) );
			  }
		 }
	}

}