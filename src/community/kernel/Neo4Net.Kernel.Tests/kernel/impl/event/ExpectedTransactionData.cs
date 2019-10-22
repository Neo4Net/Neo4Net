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
namespace Neo4Net.Kernel.Impl.@event
{

	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using LabelEntry = Neo4Net.GraphDb.Events.LabelEntry;
	using Neo4Net.GraphDb.Events;
	using TransactionData = Neo4Net.GraphDb.Events.TransactionData;
	using Neo4Net.Kernel.impl.util;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.util.AutoCreatingHashMap.nested;

	internal class ExpectedTransactionData
	{
		 internal readonly ISet<Node> ExpectedCreatedNodes = new HashSet<Node>();
		 internal readonly ISet<Relationship> ExpectedCreatedRelationships = new HashSet<Relationship>();
		 internal readonly ISet<Node> ExpectedDeletedNodes = new HashSet<Node>();
		 internal readonly ISet<Relationship> ExpectedDeletedRelationships = new HashSet<Relationship>();
		 internal readonly IDictionary<Node, IDictionary<string, PropertyEntryImpl<Node>>> ExpectedAssignedNodeProperties = new AutoCreatingHashMap<Node, IDictionary<string, PropertyEntryImpl<Node>>>( nested( typeof( string ), AutoCreatingHashMap.dontCreate<PropertyEntryImpl<Node>>() ) );
		 internal readonly IDictionary<Relationship, IDictionary<string, PropertyEntryImpl<Relationship>>> ExpectedAssignedRelationshipProperties = new AutoCreatingHashMap<Relationship, IDictionary<string, PropertyEntryImpl<Relationship>>>( nested( typeof( string ), AutoCreatingHashMap.dontCreate<PropertyEntryImpl<Relationship>>() ) );
		 internal readonly IDictionary<Node, IDictionary<string, PropertyEntryImpl<Node>>> ExpectedRemovedNodeProperties = new AutoCreatingHashMap<Node, IDictionary<string, PropertyEntryImpl<Node>>>( nested( typeof( string ), AutoCreatingHashMap.dontCreate<PropertyEntryImpl<Node>>() ) );
		 internal readonly IDictionary<Relationship, IDictionary<string, PropertyEntryImpl<Relationship>>> ExpectedRemovedRelationshipProperties = new AutoCreatingHashMap<Relationship, IDictionary<string, PropertyEntryImpl<Relationship>>>( nested( typeof( string ), AutoCreatingHashMap.dontCreate<PropertyEntryImpl<Relationship>>() ) );
		 internal readonly IDictionary<Node, ISet<string>> ExpectedAssignedLabels = new AutoCreatingHashMap<Node, ISet<string>>( AutoCreatingHashMap.valuesOfTypeHashSet<string>() );
		 internal readonly IDictionary<Node, ISet<string>> ExpectedRemovedLabels = new AutoCreatingHashMap<Node, ISet<string>>( AutoCreatingHashMap.valuesOfTypeHashSet<string>() );
		 private readonly bool _ignoreAdditionalData;

		 /// <param name="ignoreAdditionalData"> if {@code true} then only compare the expected data. If the transaction data
		 /// contains data in addition to that, then ignore that data. The reason is that for some scenarios
		 /// it's hard to anticipate the full extent of the transaction data. F.ex. deleting a node will
		 /// have all its committed properties seen as removed as well. To tell this instance about that expectancy
		 /// is difficult if there have been other property changes for that node within the same transaction
		 /// before deleting that node. It's possible, it's just that it will require some tedious state keeping
		 /// on the behalf of the test. </param>
		 internal ExpectedTransactionData( bool ignoreAdditionalData )
		 {
			  this._ignoreAdditionalData = ignoreAdditionalData;
		 }

		 internal ExpectedTransactionData() : this(false)
		 {
		 }

		 internal virtual void Clear()
		 {
			  ExpectedAssignedNodeProperties.Clear();
			  ExpectedAssignedRelationshipProperties.Clear();
			  ExpectedCreatedNodes.Clear();
			  ExpectedCreatedRelationships.Clear();
			  ExpectedDeletedNodes.Clear();
			  ExpectedDeletedRelationships.Clear();
			  ExpectedRemovedNodeProperties.Clear();
			  ExpectedRemovedRelationshipProperties.Clear();
			  ExpectedAssignedLabels.Clear();
			  ExpectedRemovedLabels.Clear();
		 }

		 internal virtual void CreatedNode( Node node )
		 {
			  ExpectedCreatedNodes.Add( node );
		 }

		 internal virtual void DeletedNode( Node node )
		 {
			  if ( !ExpectedCreatedNodes.remove( node ) )
			  {
					ExpectedDeletedNodes.Add( node );
			  }
			  ExpectedAssignedNodeProperties.Remove( node );
			  ExpectedAssignedLabels.Remove( node );
			  ExpectedRemovedNodeProperties.Remove( node );
			  ExpectedRemovedLabels.Remove( node );
		 }

		 internal virtual void CreatedRelationship( Relationship relationship )
		 {
			  ExpectedCreatedRelationships.Add( relationship );
		 }

		 internal virtual void DeletedRelationship( Relationship relationship )
		 {
			  if ( !ExpectedCreatedRelationships.remove( relationship ) )
			  {
					ExpectedDeletedRelationships.Add( relationship );
			  }
			  ExpectedAssignedRelationshipProperties.Remove( relationship );
			  ExpectedRemovedRelationshipProperties.Remove( relationship );
		 }

		 internal virtual void AssignedProperty( Node node, string key, object value, object valueBeforeTx )
		 {
			  valueBeforeTx = RemoveProperty( ExpectedRemovedNodeProperties, node, key, valueBeforeTx );
			  if ( !value.Equals( valueBeforeTx ) )
			  {
					IDictionary<string, PropertyEntryImpl<Node>> map = ExpectedAssignedNodeProperties[node];
					PropertyEntryImpl<Node> prev = map[key];
					map[key] = Property( node, key, value, prev != null ? prev.PreviouslyCommitedValue() : valueBeforeTx );
			  }
		 }

		 internal virtual void AssignedProperty( Relationship rel, string key, object value, object valueBeforeTx )
		 {
			  valueBeforeTx = RemoveProperty( ExpectedRemovedRelationshipProperties, rel, key, valueBeforeTx );
			  if ( !value.Equals( valueBeforeTx ) )
			  {
					IDictionary<string, PropertyEntryImpl<Relationship>> map = ExpectedAssignedRelationshipProperties[rel];
					PropertyEntryImpl<Relationship> prev = map[key];
					map[key] = Property( rel, key, value, prev != null ? prev.PreviouslyCommitedValue() : valueBeforeTx );
			  }
		 }

		 internal virtual void AssignedLabel( Node node, Label label )
		 {
			  if ( RemoveLabel( ExpectedRemovedLabels, node, label ) )
			  {
					ExpectedAssignedLabels[node].Add( label.Name() );
			  }
		 }

		 internal virtual void RemovedLabel( Node node, Label label )
		 {
			  if ( RemoveLabel( ExpectedAssignedLabels, node, label ) )
			  {
					ExpectedRemovedLabels[node].Add( label.Name() );
			  }
		 }

		 /// <returns> {@code true} if this property should be expected to come as removed property in the event </returns>
		 private bool RemoveLabel( IDictionary<Node, ISet<string>> map, Node node, Label label )
		 {
			  if ( map.ContainsKey( node ) )
			  {
					ISet<string> set = map[node];
					if ( !set.remove( label.Name() ) )
					{
						 return true;
					}
					if ( set.Count == 0 )
					{
						 map.Remove( node );
					}
			  }
			  return false;
		 }

		 internal virtual void RemovedProperty( Node node, string key, object valueBeforeTx )
		 {
			  if ( ( valueBeforeTx = RemoveProperty( ExpectedAssignedNodeProperties, node, key, valueBeforeTx ) ) != null )
			  {
					ExpectedRemovedNodeProperties[node][key] = Property( node, key, null, valueBeforeTx );
			  }
		 }

		 internal virtual void RemovedProperty( Relationship rel, string key, object valueBeforeTx )
		 {
			  if ( ( valueBeforeTx = RemoveProperty( ExpectedAssignedRelationshipProperties, rel, key, valueBeforeTx ) ) != null )
			  {
					ExpectedRemovedRelationshipProperties[rel][key] = Property( rel, key, null, valueBeforeTx );
			  }
		 }

		 /// <returns> {@code non-null} if this property should be expected to come as removed property in the event </returns>
		 private object RemoveProperty<E>( IDictionary<E, IDictionary<string, PropertyEntryImpl<E>>> map, E IEntity, string key, object valueBeforeTx ) where E : Neo4Net.GraphDb.PropertyContainer
		 {
			  if ( map.ContainsKey( IEntity ) )
			  {
					IDictionary<string, PropertyEntryImpl<E>> inner = map[entity];
					PropertyEntryImpl<E> entry = inner.Remove( key );
					if ( entry == null )
					{ // this means that we've been called to remove an existing property
						 return valueBeforeTx;
					}

					if ( inner.Count == 0 )
					{
						 map.Remove( IEntity );
					}
					if ( entry.PreviouslyCommitedValue() != null )
					{ // this means that we're removing a previously changed property, i.e. there's a value to remove
						 return entry.PreviouslyCommitedValue();
					}
					return null;
			  }
			  return valueBeforeTx;
		 }

		 private PropertyEntryImpl<E> Property<E>( E IEntity, string key, object value, object valueBeforeTx ) where E : Neo4Net.GraphDb.PropertyContainer
		 {
			  return new PropertyEntryImpl<E>( IEntity, key, value, valueBeforeTx );
		 }

		 internal virtual void CompareTo( TransactionData data )
		 {
			  ISet<Node> expectedCreatedNodes = new HashSet<Node>( this.ExpectedCreatedNodes );
			  ISet<Relationship> expectedCreatedRelationships = new HashSet<Relationship>( this.ExpectedCreatedRelationships );
			  ISet<Node> expectedDeletedNodes = new HashSet<Node>( this.ExpectedDeletedNodes );
			  ISet<Relationship> expectedDeletedRelationships = new HashSet<Relationship>( this.ExpectedDeletedRelationships );
			  IDictionary<Node, IDictionary<string, PropertyEntryImpl<Node>>> expectedAssignedNodeProperties = Clone( this.ExpectedAssignedNodeProperties );
			  IDictionary<Relationship, IDictionary<string, PropertyEntryImpl<Relationship>>> expectedAssignedRelationshipProperties = Clone( this.ExpectedAssignedRelationshipProperties );
			  IDictionary<Node, IDictionary<string, PropertyEntryImpl<Node>>> expectedRemovedNodeProperties = Clone( this.ExpectedRemovedNodeProperties );
			  IDictionary<Relationship, IDictionary<string, PropertyEntryImpl<Relationship>>> expectedRemovedRelationshipProperties = Clone( this.ExpectedRemovedRelationshipProperties );
			  IDictionary<Node, ISet<string>> expectedAssignedLabels = CloneLabelData( this.ExpectedAssignedLabels );
			  IDictionary<Node, ISet<string>> expectedRemovedLabels = CloneLabelData( this.ExpectedRemovedLabels );

			  foreach ( Node node in data.CreatedNodes() )
			  {
					assertTrue( expectedCreatedNodes.remove( node ) );
					assertFalse( data.IsDeleted( node ) );
			  }
			  assertTrue( "Expected some created nodes that weren't seen: " + expectedCreatedNodes, expectedCreatedNodes.Count == 0 );

			  foreach ( Relationship rel in data.CreatedRelationships() )
			  {
					assertTrue( expectedCreatedRelationships.remove( rel ) );
					assertFalse( data.IsDeleted( rel ) );
			  }
			  assertTrue( "Expected created relationships not encountered " + expectedCreatedRelationships, expectedCreatedRelationships.Count == 0 );

			  foreach ( Node node in data.DeletedNodes() )
			  {
					assertTrue( "Unexpected deleted node " + node, expectedDeletedNodes.remove( node ) );
					assertTrue( data.IsDeleted( node ) );
			  }
			  assertTrue( "Expected deleted nodes: " + expectedDeletedNodes, expectedDeletedNodes.Count == 0 );

			  foreach ( Relationship rel in data.DeletedRelationships() )
			  {
					assertTrue( expectedDeletedRelationships.remove( rel ) );
					assertTrue( data.IsDeleted( rel ) );
			  }
			  assertTrue( "Expected deleted relationships not encountered " + expectedDeletedRelationships, expectedDeletedRelationships.Count == 0 );

			  foreach ( PropertyEntry<Node> entry in data.AssignedNodeProperties() )
			  {
					CheckAssigned( expectedAssignedNodeProperties, entry );
					assertFalse( data.IsDeleted( entry.Entity() ) );
			  }
			  assertTrue( "Expected assigned node properties not encountered " + expectedAssignedNodeProperties, expectedAssignedNodeProperties.Count == 0 );

			  foreach ( PropertyEntry<Relationship> entry in data.AssignedRelationshipProperties() )
			  {
					CheckAssigned( expectedAssignedRelationshipProperties, entry );
					assertFalse( data.IsDeleted( entry.Entity() ) );
			  }
			  assertTrue( "Expected assigned relationship properties not encountered " + expectedAssignedRelationshipProperties, expectedAssignedRelationshipProperties.Count == 0 );

			  foreach ( PropertyEntry<Node> entry in data.RemovedNodeProperties() )
			  {
					CheckRemoved( expectedRemovedNodeProperties, entry );
			  }
			  assertTrue( "Expected removed node properties not encountered " + expectedRemovedNodeProperties, expectedRemovedNodeProperties.Count == 0 );

			  foreach ( PropertyEntry<Relationship> entry in data.RemovedRelationshipProperties() )
			  {
					CheckRemoved( expectedRemovedRelationshipProperties, entry );
			  }
			  assertTrue( "Expected removed relationship properties not encountered " + expectedRemovedRelationshipProperties, expectedRemovedRelationshipProperties.Count == 0 );

			  foreach ( LabelEntry entry in data.AssignedLabels() )
			  {
					Check( expectedAssignedLabels, entry );
			  }
			  assertTrue( "Expected assigned labels not encountered " + expectedAssignedLabels, expectedAssignedLabels.Count == 0 );

			  foreach ( LabelEntry entry in data.RemovedLabels() )
			  {
					Check( expectedRemovedLabels, entry );
			  }
			  assertTrue( "Expected removed labels not encountered " + expectedRemovedLabels, expectedRemovedLabels.Count == 0 );
		 }

		 private IDictionary<Node, ISet<string>> CloneLabelData( IDictionary<Node, ISet<string>> map )
		 {
			  IDictionary<Node, ISet<string>> clone = new Dictionary<Node, ISet<string>>();
			  foreach ( KeyValuePair<Node, ISet<string>> entry in map.SetOfKeyValuePairs() )
			  {
					clone[entry.Key] = new HashSet<string>( entry.Value );
			  }
			  return clone;
		 }

		 private void Check( IDictionary<Node, ISet<string>> expected, LabelEntry entry )
		 {
			  Node node = entry.Node();
			  string labelName = entry.Label().name();
			  bool hasEntity = expected.ContainsKey( node );
			  if ( !hasEntity && _ignoreAdditionalData )
			  {
					return;
			  }
			  assertTrue( "Unexpected node " + node, hasEntity );
			  ISet<string> labels = expected[node];
			  bool hasLabel = labels.remove( labelName );
			  if ( !hasLabel && _ignoreAdditionalData )
			  {
					return;
			  }
			  assertTrue( "Unexpected label " + labelName + " for " + node, hasLabel );
			  if ( labels.Count == 0 )
			  {
					expected.Remove( node );
			  }
		 }

		 private IDictionary<KEY, IDictionary<string, PropertyEntryImpl<KEY>>> Clone<KEY>( IDictionary<KEY, IDictionary<string, PropertyEntryImpl<KEY>>> map ) where KEY : Neo4Net.GraphDb.PropertyContainer
		 {
			  IDictionary<KEY, IDictionary<string, PropertyEntryImpl<KEY>>> result = new Dictionary<KEY, IDictionary<string, PropertyEntryImpl<KEY>>>();
			  foreach ( KEY key in map.Keys )
			  {
					result[key] = new Dictionary<string, PropertyEntryImpl<KEY>>( map[key] );
			  }
			  return result;
		 }

		 internal virtual void CheckAssigned<T>( IDictionary<T, IDictionary<string, PropertyEntryImpl<T>>> map, PropertyEntry<T> entry ) where T : Neo4Net.GraphDb.PropertyContainer
		 {
			  PropertyEntryImpl<T> expected = FetchExpectedPropertyEntry( map, entry );
			  if ( expected != null )
			  { // To handle the ignore flag (read above)
					expected.CompareToAssigned( entry );
			  }
		 }

		 internal virtual void CheckRemoved<T>( IDictionary<T, IDictionary<string, PropertyEntryImpl<T>>> map, PropertyEntry<T> entry ) where T : Neo4Net.GraphDb.PropertyContainer
		 {
			  PropertyEntryImpl<T> expected = FetchExpectedPropertyEntry( map, entry );
			  if ( expected != null )
			  { // To handle the ignore flag (read above)
					expected.CompareToRemoved( entry );
			  }
		 }

		 internal virtual PropertyEntryImpl<T> FetchExpectedPropertyEntry<T>( IDictionary<T, IDictionary<string, PropertyEntryImpl<T>>> map, PropertyEntry<T> entry ) where T : Neo4Net.GraphDb.PropertyContainer
		 {
			  T IEntity = entry.Entity();
			  bool hasEntity = map.ContainsKey( IEntity );
			  if ( _ignoreAdditionalData && !hasEntity )
			  {
					return null;
			  }
			  assertTrue( "Unexpected IEntity " + entry, hasEntity );
			  IDictionary<string, PropertyEntryImpl<T>> innerMap = map[entity];
			  PropertyEntryImpl<T> expectedEntry = innerMap.Remove( entry.Key() );
			  if ( expectedEntry == null && _ignoreAdditionalData )
			  {
					return null;
			  }
			  assertNotNull( "Unexpected property entry " + entry, expectedEntry );
			  if ( innerMap.Count == 0 )
			  {
					map.Remove( IEntity );
			  }
			  return expectedEntry;
		 }
	}

}