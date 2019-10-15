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
namespace Neo4Net.Kernel.impl.coreapi.schema
{

	using ConstraintViolationException = Neo4Net.Graphdb.ConstraintViolationException;
	using Label = Neo4Net.Graphdb.Label;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using IndexDefinition = Neo4Net.Graphdb.schema.IndexDefinition;
	using IHashFunction = Neo4Net.Hashing.HashFunction;
	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using TokenRead = Neo4Net.Internal.Kernel.Api.TokenRead;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.stream;

	public class IndexDefinitionImpl : IndexDefinition
	{
		 private readonly InternalSchemaActions _actions;

		 private readonly IndexReference _indexReference;
		 private readonly Label[] _labels;
		 private readonly RelationshipType[] _relTypes;
		 private readonly string[] _propertyKeys;
		 private readonly bool _constraintIndex;

		 public IndexDefinitionImpl( InternalSchemaActions actions, IndexReference @ref, Label[] labels, string[] propertyKeys, bool constraintIndex )
		 {
			  this._actions = actions;
			  this._indexReference = @ref;
			  this._labels = labels;
			  this._relTypes = null;
			  this._propertyKeys = propertyKeys;
			  this._constraintIndex = constraintIndex;

			  AssertInUnterminatedTransaction();
		 }

		 public IndexDefinitionImpl( InternalSchemaActions actions, IndexReference @ref, RelationshipType[] relTypes, string[] propertyKeys, bool constraintIndex )
		 {
			  this._actions = actions;
			  this._indexReference = @ref;
			  this._labels = null;
			  this._relTypes = relTypes;
			  this._propertyKeys = propertyKeys;
			  this._constraintIndex = constraintIndex;

			  AssertInUnterminatedTransaction();
		 }

		 public virtual IndexReference IndexReference
		 {
			 get
			 {
				  return _indexReference;
			 }
		 }

		 public virtual Label Label
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  AssertIsNodeIndex();
				  if ( _labels.Length > 1 )
				  {
						throw new System.InvalidOperationException( "This is a multi-token index, which has more than one label. Call the getLabels() method instead." );
				  }
				  return _labels[0];
			 }
		 }

		 public virtual IEnumerable<Label> Labels
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  AssertIsNodeIndex();
				  return Arrays.asList( _labels );
			 }
		 }

		 public virtual RelationshipType RelationshipType
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  AssertIsRelationshipIndex();
				  if ( _relTypes.Length > 1 )
				  {
						throw new System.InvalidOperationException( "This is a multi-token index, which has more than one relationship type. " + "Call the getRelationshipTypes() method instead." );
				  }
				  return _relTypes[0];
			 }
		 }

		 public virtual IEnumerable<RelationshipType> RelationshipTypes
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  AssertIsRelationshipIndex();
				  return Arrays.asList( _relTypes );
			 }
		 }

		 public virtual IEnumerable<string> PropertyKeys
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  return asList( _propertyKeys );
			 }
		 }

		 /// <summary>
		 /// Returns the inner array of property keys in this index definition.
		 /// <para>
		 /// This array <em><strong>must not</strong></em> be modified, since the index definition is supposed to be immutable.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> The array of property keys. </returns>
		 internal virtual string[] PropertyKeysArrayShared
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  return _propertyKeys;
			 }
		 }

		 /// <summary>
		 /// Returns the inner array of labels in this index definition.
		 /// <para>
		 /// This array <em><strong>must not</strong></em> be modified, since the index definition is supposed to be immutable.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> The label array, which may be null. </returns>
		 internal virtual Label[] LabelArrayShared
		 {
			 get
			 {
				  return _labels;
			 }
		 }

		 /// <summary>
		 /// Returns the inner array of relationship types in this index definition.
		 /// <para>
		 /// This array <em><strong>must not</strong></em> be modified, since the index definition is supposed to be immutable.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> The relationship type array, which may be null. </returns>
		 internal virtual RelationshipType[] RelationshipTypesArrayShared
		 {
			 get
			 {
				  return _relTypes;
			 }
		 }

		 public override void Drop()
		 {
			  try
			  {
					_actions.dropIndexDefinitions( this );
			  }
			  catch ( ConstraintViolationException e )
			  {
					if ( this.ConstraintIndex )
					{
						 throw new System.InvalidOperationException( "Constraint indexes cannot be dropped directly, " + "instead drop the owning uniqueness constraint.", e );
					}
					throw e;
			  }
		 }

		 public virtual bool ConstraintIndex
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  return _constraintIndex;
			 }
		 }

		 public virtual bool NodeIndex
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  return InternalIsNodeIndex();
			 }
		 }

		 private bool InternalIsNodeIndex()
		 {
			  return _labels != null;
		 }

		 public virtual bool RelationshipIndex
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  return _relTypes != null;
			 }
		 }

		 public virtual bool MultiTokenIndex
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  return InternalIsNodeIndex() ? _labels.Length > 1 : _relTypes.Length > 1;
			 }
		 }

		 public virtual bool CompositeIndex
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  return _propertyKeys.Length > 1;
			 }
		 }

		 public virtual string Name
		 {
			 get
			 {
				  return _indexReference == null ? Neo4Net.Internal.Kernel.Api.IndexReference_Fields.UNNAMED_INDEX : _indexReference.name();
			 }
		 }

		 public override int GetHashCode()
		 {
			  IHashFunction hf = HashFunctionHelper.IncrementalXXH64();
			  long hash = hf.Initialize( 31 );
			  hash = hf.UpdateWithArray( hash, _labels, label => label.name().GetHashCode() );
			  hash = hf.UpdateWithArray( hash, _relTypes, relType => relType.name().GetHashCode() );
			  hash = hf.UpdateWithArray( hash, _propertyKeys, string.hashCode );
			  return hf.ToInt( hash );
		 }

		 public override bool Equals( object obj )
		 {
			  if ( this == obj )
			  {
					return true;
			  }
			  if ( obj == null )
			  {
					return false;
			  }
			  if ( this.GetType() != obj.GetType() )
			  {
					return false;
			  }
			  IndexDefinitionImpl other = ( IndexDefinitionImpl ) obj;
			  if ( InternalIsNodeIndex() )
			  {
					if ( other._labels == null )
					{
						 return false;
					}
					if ( _labels.Length != other._labels.Length )
					{
						 return false;
					}
					for ( int i = 0; i < _labels.Length; i++ )
					{
						 if ( !_labels[i].name().Equals(other._labels[i].name()) )
						 {
							  return false;
						 }
					}
			  }
			  if ( _relTypes != null )
			  {
					if ( other._relTypes == null )
					{
						 return false;
					}
					if ( _relTypes.Length != other._relTypes.Length )
					{
						 return false;
					}
					for ( int i = 0; i < _relTypes.Length; i++ )
					{
						 if ( !_relTypes[i].name().Equals(other._relTypes[i].name()) )
						 {
							  return false;
						 }
					}
			  }
			  return Arrays.Equals( _propertyKeys, other._propertyKeys );
		 }

		 public override string ToString()
		 {
			  string entityTokenType;
			  string entityTokens;
			  if ( InternalIsNodeIndex() )
			  {
					entityTokenType = _labels.Length > 1 ? "labels" : "label";
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					entityTokens = java.util.labels.Select( Label::name ).collect( joining( "," ) );
			  }
			  else
			  {
					entityTokenType = _relTypes.Length > 1 ? "relationship types" : "relationship type";
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					entityTokens = java.util.relTypes.Select( RelationshipType::name ).collect( joining( "," ) );
			  }
			  return "IndexDefinition[" + entityTokenType + ":" + entityTokens + " on:" + string.join( ",", _propertyKeys ) + "]" + ( _indexReference == null ? "" : " (" + _indexReference + ")" );
		 }

		 public static string LabelNameList( IEnumerable<Label> labels, string prefix, string postfix )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return stream( labels ).map( Label::name ).collect( joining( ", ", prefix, postfix ) );
		 }

		 private void AssertInUnterminatedTransaction()
		 {
			  _actions.assertInOpenTransaction();
		 }

		 private void AssertIsNodeIndex()
		 {
			  if ( !NodeIndex )
			  {
					throw new System.InvalidOperationException( "This is not a node index." );
			  }
		 }

		 private void AssertIsRelationshipIndex()
		 {
			  if ( !RelationshipIndex )
			  {
					throw new System.InvalidOperationException( "This is not a relationship index." );
			  }
		 }
	}

}