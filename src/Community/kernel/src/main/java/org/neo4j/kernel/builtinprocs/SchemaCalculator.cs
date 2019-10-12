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
namespace Neo4Net.Kernel.builtinprocs
{
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;
	using IntSets = org.eclipse.collections.impl.factory.primitive.IntSets;
	using IntHashSet = org.eclipse.collections.impl.set.mutable.primitive.IntHashSet;


	using Neo4Net.Helpers.Collection;
	using CursorFactory = Neo4Net.@internal.Kernel.Api.CursorFactory;
	using NamedToken = Neo4Net.@internal.Kernel.Api.NamedToken;
	using NodeCursor = Neo4Net.@internal.Kernel.Api.NodeCursor;
	using PropertyCursor = Neo4Net.@internal.Kernel.Api.PropertyCursor;
	using Read = Neo4Net.@internal.Kernel.Api.Read;
	using RelationshipScanCursor = Neo4Net.@internal.Kernel.Api.RelationshipScanCursor;
	using TokenRead = Neo4Net.@internal.Kernel.Api.TokenRead;
	using Transaction = Neo4Net.@internal.Kernel.Api.Transaction;
	using Value = Neo4Net.Values.Storable.Value;

	public class SchemaCalculator
	{
		 private IDictionary<int, string> _propertyIdToPropertyNameMapping;

		 private readonly MutableIntSet _emptyPropertyIdSet = IntSets.mutable.empty();

		 private readonly Read _dataRead;
		 private readonly TokenRead _tokenRead;
		 private readonly CursorFactory _cursors;

		 internal SchemaCalculator( Transaction ktx )
		 {
			  this._dataRead = ktx.DataRead();
			  this._tokenRead = ktx.TokenRead();
			  this._cursors = ktx.Cursors();

			  // the only one that is common for both nodes and rels so thats why we can do it here
			  _propertyIdToPropertyNameMapping = new Dictionary<int, string>( _tokenRead.propertyKeyCount() );
			  AddNamesToCollection( _tokenRead.propertyKeyGetAllTokens(), _propertyIdToPropertyNameMapping );
		 }

		 private NodeMappings InitializeMappingsForNodes()
		 {
			  int labelCount = _tokenRead.labelCount();
			  return new NodeMappings( this, labelCount );
		 }

		 private RelationshipMappings InitializeMappingsForRels()
		 {
			  int relationshipTypeCount = _tokenRead.relationshipTypeCount();
			  return new RelationshipMappings( this, relationshipTypeCount );
		 }

		 // If we would have this schema information in the count store (or somewhere), this could be super fast
		 public virtual Stream<NodePropertySchemaInfoResult> CalculateTabularResultStreamForNodes()
		 {
			  NodeMappings nodeMappings = InitializeMappingsForNodes();
			  ScanEverythingBelongingToNodes( nodeMappings );

			  // go through all labels to get actual names
			  AddNamesToCollection( _tokenRead.labelsGetAllTokens(), nodeMappings.LabelIdToLabelName );

			  return ProduceResultsForNodes( nodeMappings ).stream();
		 }

		 public virtual Stream<RelationshipPropertySchemaInfoResult> CalculateTabularResultStreamForRels()
		 {
			  RelationshipMappings relMappings = InitializeMappingsForRels();
			  ScanEverythingBelongingToRelationships( relMappings );

			  // go through all relationshipTypes to get actual names
			  AddNamesToCollection( _tokenRead.relationshipTypesGetAllTokens(), relMappings.RelationshipTypIdToRelationshipName );

			  return ProduceResultsForRelationships( relMappings ).stream();
		 }

		 private IList<RelationshipPropertySchemaInfoResult> ProduceResultsForRelationships( RelationshipMappings relMappings )
		 {
			  IList<RelationshipPropertySchemaInfoResult> results = new List<RelationshipPropertySchemaInfoResult>();
			  foreach ( int? typeId in relMappings.RelationshipTypeIdToPropertyKeys.Keys )
			  {
					// lookup typ name
					string name = relMappings.RelationshipTypIdToRelationshipName[typeId];
					name = ":`" + name + "`"; // escaping

					// lookup property value types
					MutableIntSet propertyIds = relMappings.RelationshipTypeIdToPropertyKeys[typeId];
					if ( propertyIds.size() == 0 )
					{
						 results.Add( new RelationshipPropertySchemaInfoResult( name, null, null, false ) );
					}
					else
					{
						 string finalName = name;
						 propertyIds.forEach(propId =>
						 {
						 string propName = _propertyIdToPropertyNameMapping[propId];
						 ValueTypeListHelper valueTypeListHelper = relMappings.RelationshipTypeIdANDPropertyTypeIdToValueType[Pair.of( typeId, propId )];
						 if ( relMappings.NullableRelationshipTypes.Contains( typeId ) )
						 {
							 results.Add( new RelationshipPropertySchemaInfoResult( finalName, propName, valueTypeListHelper.CypherTypesList, false ) );
						 }
						 else
						 {
							 results.Add( new RelationshipPropertySchemaInfoResult( finalName, propName, valueTypeListHelper.CypherTypesList, valueTypeListHelper.Mandatory ) );
						 }
						 });
					}
			  }
			  return results;
		 }

		 private IList<NodePropertySchemaInfoResult> ProduceResultsForNodes( NodeMappings nodeMappings )
		 {
			  IList<NodePropertySchemaInfoResult> results = new List<NodePropertySchemaInfoResult>();
			  foreach ( SortedLabels labelSet in nodeMappings.LabelSetToPropertyKeys.Keys )
			  {
					// lookup label names and produce list of names and produce String out of them
					IList<string> labelNames = new List<string>();
					for ( int i = 0; i < labelSet.NumberOfLabels(); i++ )
					{
						 string name = nodeMappings.LabelIdToLabelName[labelSet.Label( i )];
						 labelNames.Add( name );
					}
					labelNames.Sort(); // this is optional but waaaaay nicer
					StringBuilder labelsConcatenator = new StringBuilder();
					foreach ( string item in labelNames )
					{
						 labelsConcatenator.Append( ":`" ).Append( item ).Append( "`" );
					}
					string labels = labelsConcatenator.ToString();

					// lookup property value types
					MutableIntSet propertyIds = nodeMappings.LabelSetToPropertyKeys[labelSet];
					if ( propertyIds.size() == 0 )
					{
						 results.Add( new NodePropertySchemaInfoResult( labels, labelNames, null, null, false ) );
					}
					else
					{
						 propertyIds.forEach(propId =>
						 {
						 string propName = _propertyIdToPropertyNameMapping[propId];
						 ValueTypeListHelper valueTypeListHelper = nodeMappings.LabelSetANDNodePropertyKeyIdToValueType[Pair.of( labelSet, propId )];
						 if ( nodeMappings.NullableLabelSets.Contains( labelSet ) )
						 {
							 results.Add( new NodePropertySchemaInfoResult( labels, labelNames, propName, valueTypeListHelper.CypherTypesList, false ) );
						 }
						 else
						 {
							 results.Add( new NodePropertySchemaInfoResult( labels, labelNames, propName, valueTypeListHelper.CypherTypesList, valueTypeListHelper.Mandatory ) );
						 }
						 });
					}
			  }
			  return results;
		 }

		 private void ScanEverythingBelongingToRelationships( RelationshipMappings relMappings )
		 {
			  using ( RelationshipScanCursor relationshipScanCursor = _cursors.allocateRelationshipScanCursor(), PropertyCursor propertyCursor = _cursors.allocatePropertyCursor() )
			  {
					_dataRead.allRelationshipsScan( relationshipScanCursor );
					while ( relationshipScanCursor.Next() )
					{
						 int typeId = relationshipScanCursor.Type();
						 relationshipScanCursor.Properties( propertyCursor );
						 MutableIntSet propertyIds = IntSets.mutable.empty();

						 while ( propertyCursor.Next() )
						 {
							  int propertyKey = propertyCursor.PropertyKey();

							  Value currentValue = propertyCursor.PropertyValue();
							  Pair<int, int> key = Pair.of( typeId, propertyKey );
							  UpdateValueTypeInMapping( currentValue, key, relMappings.RelationshipTypeIdANDPropertyTypeIdToValueType );

							  propertyIds.add( propertyKey );
						 }
						 propertyCursor.Close();

						 MutableIntSet oldPropertyKeySet = relMappings.RelationshipTypeIdToPropertyKeys.getOrDefault( typeId, _emptyPropertyIdSet );

						 // find out which old properties we did not visited and mark them as nullable
						 if ( oldPropertyKeySet == _emptyPropertyIdSet )
						 {
							  if ( propertyIds.size() == 0 )
							  {
									// Even if we find property key on other rels with this type, set all of them nullable
									relMappings.NullableRelationshipTypes.Add( typeId );
							  }

							  propertyIds.addAll( oldPropertyKeySet );
						 }
						 else
						 {
							  MutableIntSet currentPropertyIdsHelperSet = new IntHashSet( propertyIds.size() );
							  currentPropertyIdsHelperSet.addAll( propertyIds );
							  propertyIds.removeAll( oldPropertyKeySet ); // only the brand new ones in propIds now
							  oldPropertyKeySet.removeAll( currentPropertyIdsHelperSet ); // only the old ones that are not on the new rel

							  propertyIds.addAll( oldPropertyKeySet );
							  propertyIds.forEach(id =>
							  {
							  Pair<int, int> key = Pair.of( typeId, id );
							  relMappings.RelationshipTypeIdANDPropertyTypeIdToValueType[key].setNullable();
							  });

							  propertyIds.addAll( currentPropertyIdsHelperSet );
						 }

						 relMappings.RelationshipTypeIdToPropertyKeys[typeId] = propertyIds;
					}
					relationshipScanCursor.Close();
			  }
		 }

		 private void ScanEverythingBelongingToNodes( NodeMappings nodeMappings )
		 {
			  using ( NodeCursor nodeCursor = _cursors.allocateNodeCursor(), PropertyCursor propertyCursor = _cursors.allocatePropertyCursor() )
			  {
					_dataRead.allNodesScan( nodeCursor );
					while ( nodeCursor.Next() )
					{
						 // each node
						 SortedLabels labels = SortedLabels.From( nodeCursor.Labels() );
						 nodeCursor.Properties( propertyCursor );
						 MutableIntSet propertyIds = IntSets.mutable.empty();

						 while ( propertyCursor.Next() )
						 {
							  Value currentValue = propertyCursor.PropertyValue();
							  int propertyKeyId = propertyCursor.PropertyKey();
							  Pair<SortedLabels, int> key = Pair.of( labels, propertyKeyId );
							  UpdateValueTypeInMapping( currentValue, key, nodeMappings.LabelSetANDNodePropertyKeyIdToValueType );

							  propertyIds.add( propertyKeyId );
						 }
						 propertyCursor.Close();

						 MutableIntSet oldPropertyKeySet = nodeMappings.LabelSetToPropertyKeys.getOrDefault( labels, _emptyPropertyIdSet );

						 // find out which old properties we did not visited and mark them as nullable
						 if ( oldPropertyKeySet == _emptyPropertyIdSet )
						 {
							  if ( propertyIds.size() == 0 )
							  {
									// Even if we find property key on other nodes with those labels, set all of them nullable
									nodeMappings.NullableLabelSets.Add( labels );
							  }

							  propertyIds.addAll( oldPropertyKeySet );
						 }
						 else
						 {
							  MutableIntSet currentPropertyIdsHelperSet = new IntHashSet( propertyIds.size() );
							  currentPropertyIdsHelperSet.addAll( propertyIds );
							  propertyIds.removeAll( oldPropertyKeySet ); // only the brand new ones in propIds now
							  oldPropertyKeySet.removeAll( currentPropertyIdsHelperSet ); // only the old ones that are not on the new node

							  propertyIds.addAll( oldPropertyKeySet );
							  propertyIds.forEach(id =>
							  {
							  Pair<SortedLabels, int> key = Pair.of( labels, id );
							  nodeMappings.LabelSetANDNodePropertyKeyIdToValueType[key].setNullable();
							  });

							  propertyIds.addAll( currentPropertyIdsHelperSet );
						 }

						 nodeMappings.LabelSetToPropertyKeys[labels] = propertyIds;
					}
					nodeCursor.Close();
			  }
		 }

		 private void UpdateValueTypeInMapping<X, Y>( Value currentValue, Pair<X, Y> key, IDictionary<Pair<X, Y>, ValueTypeListHelper> mappingToUpdate )
		 {
			  ValueTypeListHelper helper = mappingToUpdate[key];
			  if ( helper == null )
			  {
					helper = new ValueTypeListHelper( this, currentValue );
					mappingToUpdate[key] = helper;
			  }
			  else
			  {
					helper.UpdateValueTypesWith( currentValue );
			  }
		 }

		 private void AddNamesToCollection( IEnumerator<NamedToken> labelIterator, IDictionary<int, string> collection )
		 {
			  while ( labelIterator.MoveNext() )
			  {
					NamedToken label = labelIterator.Current;
					collection[label.Id()] = label.Name();
			  }
		 }

		 private class ValueTypeListHelper
		 {
			 private readonly SchemaCalculator _outerInstance;

			  internal ISet<string> SeenValueTypes;
			  internal bool IsMandatory = true;

			  internal ValueTypeListHelper( SchemaCalculator outerInstance, Value v )
			  {
				  this._outerInstance = outerInstance;
					SeenValueTypes = new HashSet<string>();
					UpdateValueTypesWith( v );
			  }

			  internal virtual void SetNullable()
			  {
						 IsMandatory = false;
			  }

			  public virtual bool Mandatory
			  {
				  get
				  {
						return IsMandatory;
				  }
			  }

			  internal virtual IList<string> CypherTypesList
			  {
				  get
				  {
						return new List<string>( SeenValueTypes );
				  }
			  }

			  internal virtual void UpdateValueTypesWith( Value newValue )
			  {
					if ( newValue == null )
					{
						 throw new System.ArgumentException();
					}
					SeenValueTypes.Add( newValue.TypeName );
			  }
		 }

		 /*
		   All mappings needed to describe Nodes except for property infos
		  */
		 private class NodeMappings
		 {
			 private readonly SchemaCalculator _outerInstance;

			  internal readonly IDictionary<SortedLabels, MutableIntSet> LabelSetToPropertyKeys;
			  internal readonly IDictionary<Pair<SortedLabels, int>, ValueTypeListHelper> LabelSetANDNodePropertyKeyIdToValueType;
			  internal readonly ISet<SortedLabels> NullableLabelSets; // used for label combinations without properties -> all properties are viewed as nullable
			  internal readonly IDictionary<int, string> LabelIdToLabelName;

			  internal NodeMappings( SchemaCalculator outerInstance, int labelCount )
			  {
				  this._outerInstance = outerInstance;
					LabelSetToPropertyKeys = new Dictionary<SortedLabels, MutableIntSet>( labelCount );
					LabelIdToLabelName = new Dictionary<int, string>( labelCount );
					LabelSetANDNodePropertyKeyIdToValueType = new Dictionary<Pair<SortedLabels, int>, ValueTypeListHelper>();
					NullableLabelSets = new HashSet<SortedLabels>();
			  }
		 }

		 /*
		   All mappings needed to describe Rels except for property infos
		  */
		 private class RelationshipMappings
		 {
			 private readonly SchemaCalculator _outerInstance;

			  internal readonly IDictionary<int, string> RelationshipTypIdToRelationshipName;
			  internal readonly IDictionary<int, MutableIntSet> RelationshipTypeIdToPropertyKeys;
			  internal readonly IDictionary<Pair<int, int>, ValueTypeListHelper> RelationshipTypeIdANDPropertyTypeIdToValueType;
			  internal readonly ISet<int> NullableRelationshipTypes; // used for types without properties -> all properties are viewed as nullable

			  internal RelationshipMappings( SchemaCalculator outerInstance, int relationshipTypeCount )
			  {
				  this._outerInstance = outerInstance;
					RelationshipTypIdToRelationshipName = new Dictionary<int, string>( relationshipTypeCount );
					RelationshipTypeIdToPropertyKeys = new Dictionary<int, MutableIntSet>( relationshipTypeCount );
					RelationshipTypeIdANDPropertyTypeIdToValueType = new Dictionary<Pair<int, int>, ValueTypeListHelper>();
					NullableRelationshipTypes = new HashSet<int>();
			  }
		 }
	}

}