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
namespace Neo4Net.Kernel.Impl.Api.state
{
	using IntIterable = org.eclipse.collections.api.IntIterable;
	using IntList = org.eclipse.collections.api.list.primitive.IntList;
	using IntSets = org.eclipse.collections.impl.factory.primitive.IntSets;
	using Matcher = org.hamcrest.Matcher;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using PropertyKeyValue = Neo4Net.Kernel.api.properties.PropertyKeyValue;
	using TransactionState = Neo4Net.Kernel.api.txstate.TransactionState;
	using StorageProperty = Neo4Net.Kernel.Api.StorageEngine.StorageProperty;
	using TxStateVisitor = Neo4Net.Kernel.Api.StorageEngine.TxState.TxStateVisitor;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class TxStateVisitorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeAddedRelationshipProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeAddedRelationshipProperties()
		 {
			  // Given
			  long relId = 1L;
			  int propKey = 2;
			  GatheringVisitor visitor = new GatheringVisitor();
			  Value value = Values.of( "hello" );
			  _state.relationshipDoReplaceProperty( relId, propKey, Values.of( "" ), value );

			  // When
			  _state.accept( visitor );

			  // Then
			  StorageProperty prop = new PropertyKeyValue( propKey, Values.of( "hello" ) );
			  assertThat( visitor.RelPropertyChanges, Contains( PropChange( relId, _noProperty, new IList<StorageProperty> { prop }, IntSets.immutable.empty() ) ) );
		 }

		 private Matcher<IList<GatheringVisitor.PropertyChange>> Contains( params GatheringVisitor.PropertyChange[] change )
		 {
			  return equalTo( asList( change ) );
		 }

		 private GatheringVisitor.PropertyChange PropChange( long relId, ICollection<StorageProperty> added, IList<StorageProperty> changed, IntIterable removed )
		 {
			  return new GatheringVisitor.PropertyChange( relId, added, changed, removed );
		 }

		 private TransactionState _state;
		 private readonly ICollection<StorageProperty> _noProperty = Collections.emptySet();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _state = new TxState();
		 }

		 internal class GatheringVisitor : Neo4Net.Kernel.Api.StorageEngine.TxState.TxStateVisitor_Adapter
		 {
			  internal class PropertyChange
			  {
					internal readonly long IEntityId;
					internal readonly IList<StorageProperty> Added;
					internal readonly IList<StorageProperty> Changed;
					internal readonly IntList Removed;

					internal PropertyChange( long IEntityId, ICollection<StorageProperty> added, ICollection<StorageProperty> changed, IntIterable removed )
					{
						 this.EntityId = IEntityId;
						 this.Added = Iterables.asList( added );
						 this.Changed = Iterables.asList( changed );
						 this.Removed = removed.toList();
					}

					internal PropertyChange( long IEntityId, IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
					{
						 this.EntityId = IEntityId;
						 this.Added = Iterators.asList( added );
						 this.Changed = Iterators.asList( changed );
						 this.Removed = removed.toList();
					}

					public override string ToString()
					{
						 return "PropertyChange{" +
									"entityId=" + IEntityId +
									", added=" + Added +
									", changed=" + Changed +
									", removed=" + Removed +
									'}';
					}

					public override bool Equals( object o )
					{
						 if ( this == o )
						 {
							  return true;
						 }
						 if ( o == null || this.GetType() != o.GetType() )
						 {
							  return false;
						 }

						 PropertyChange that = ( PropertyChange ) o;

						 if ( IEntityId != that.EntityId )
						 {
							  return false;
						 }
//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: if (!added.equals(that.added))
						 if ( !Added.SequenceEqual( that.Added ) )
						 {
							  return false;
						 }
//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: if (!changed.equals(that.changed))
						 if ( !Changed.SequenceEqual( that.Changed ) )
						 {
							  return false;
						 }
						 return Removed.Equals( that.Removed );
					}

					public override int GetHashCode()
					{
						 int result = ( int )( IEntityId ^ ( ( long )( ( ulong )EntityId >> 32 ) ) );
						 result = 31 * result + Added.GetHashCode();
						 result = 31 * result + Changed.GetHashCode();
						 result = 31 * result + Removed.GetHashCode();
						 return result;
					}
			  }

			  public IList<PropertyChange> NodePropertyChanges = new List<PropertyChange>();
			  public IList<PropertyChange> RelPropertyChanges = new List<PropertyChange>();
			  public IList<PropertyChange> GraphPropertyChanges = new List<PropertyChange>();

			  public override void VisitNodePropertyChanges( long id, IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
			  {
					NodePropertyChanges.Add( new PropertyChange( id, added, changed, removed ) );
			  }

			  public override void VisitRelPropertyChanges( long id, IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
			  {
					RelPropertyChanges.Add( new PropertyChange( id, added, changed, removed ) );
			  }

			  public override void VisitGraphPropertyChanges( IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
			  {
					GraphPropertyChanges.Add( new PropertyChange( -1, added, changed, removed ) );
			  }
		 }
	}

}