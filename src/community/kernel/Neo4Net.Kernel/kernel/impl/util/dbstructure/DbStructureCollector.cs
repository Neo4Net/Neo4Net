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
namespace Neo4Net.Kernel.impl.util.dbstructure
{
	using MutableIntLongMap = org.eclipse.collections.api.map.primitive.MutableIntLongMap;
	using IntLongHashMap = org.eclipse.collections.impl.map.mutable.primitive.IntLongHashMap;


	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using Neo4Net.Helpers.Collections;
	using LabelSchemaSupplier = Neo4Net.Kernel.Api.Internal.schema.LabelSchemaSupplier;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using NodeExistenceConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.NodeExistenceConstraintDescriptor;
	using NodeKeyConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.NodeKeyConstraintDescriptor;
	using RelExistenceConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.RelExistenceConstraintDescriptor;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.UniquenessConstraintDescriptor;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor.Type.UNIQUE;

	public class DbStructureCollector : DbStructureVisitor
	{
		private bool InstanceFieldsInitialized = false;

		public DbStructureCollector()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_regularIndices = new IndexDescriptorMap( this, "regular" );
			_uniqueIndices = new IndexDescriptorMap( this, "unique" );
		}

		 private readonly TokenMap _labels = new TokenMap( "label" );
		 private readonly TokenMap _propertyKeys = new TokenMap( "property key" );
		 private readonly TokenMap _relationshipTypes = new TokenMap( "relationship types" );
		 private IndexDescriptorMap _regularIndices;
		 private IndexDescriptorMap _uniqueIndices;
		 private readonly ISet<UniquenessConstraintDescriptor> _uniquenessConstraints = new HashSet<UniquenessConstraintDescriptor>();
		 private readonly ISet<NodeExistenceConstraintDescriptor> _nodePropertyExistenceConstraints = new HashSet<NodeExistenceConstraintDescriptor>();
		 private readonly ISet<RelExistenceConstraintDescriptor> _relPropertyExistenceConstraints = new HashSet<RelExistenceConstraintDescriptor>();
		 private readonly ISet<NodeKeyConstraintDescriptor> _nodeKeyConstraints = new HashSet<NodeKeyConstraintDescriptor>();
		 private readonly MutableIntLongMap _nodeCounts = new IntLongHashMap();
		 private readonly IDictionary<RelSpecifier, long> _relCounts = new Dictionary<RelSpecifier, long>();
		 private long _allNodesCount = -1L;

		 public virtual DbStructureLookup Lookup()
		 {
			  return new DbStructureLookupAnonymousInnerClass( this );
		 }

		 private class DbStructureLookupAnonymousInnerClass : DbStructureLookup
		 {
			 private readonly DbStructureCollector _outerInstance;

			 public DbStructureLookupAnonymousInnerClass( DbStructureCollector outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public IEnumerator<Pair<int, string>> labels()
			 {
				  return _outerInstance.labels.GetEnumerator();
			 }

			 public IEnumerator<Pair<int, string>> properties()
			 {
				  return _outerInstance.propertyKeys.GetEnumerator();
			 }

			 public IEnumerator<Pair<int, string>> relationshipTypes()
			 {
				  return _outerInstance.relationshipTypes.GetEnumerator();
			 }

			 public IEnumerator<Pair<string[], string[]>> knownIndices()
			 {
				  return _outerInstance.regularIndices.GetEnumerator();
			 }

			 public IEnumerator<Pair<string[], string[]>> knownUniqueIndices()
			 {
				  return _outerInstance.uniqueIndices.GetEnumerator();
			 }

			 public IEnumerator<Pair<string, string[]>> knownUniqueConstraints()
			 {
				  return idsToNames( _outerInstance.uniquenessConstraints );
			 }

			 public IEnumerator<Pair<string, string[]>> knownNodePropertyExistenceConstraints()
			 {
				  return idsToNames( _outerInstance.nodePropertyExistenceConstraints );
			 }

			 public IEnumerator<Pair<string, string[]>> knownNodeKeyConstraints()
			 {
				  return idsToNames( _outerInstance.nodeKeyConstraints );
			 }

			 public long nodesAllCardinality()
			 {
				  return _outerInstance.allNodesCount;
			 }

			 public IEnumerator<Pair<string, string[]>> knownRelationshipPropertyExistenceConstraints()
			 {
				  return Iterators.map(relConstraint =>
				  {
					string label = _outerInstance.labels.byIdOrFail( relConstraint.schema().RelTypeId );
					string[] propertyKeyNames = _outerInstance.propertyKeys.byIdOrFail( relConstraint.schema().PropertyIds );
					return Pair.of( label, propertyKeyNames );
				  }, _outerInstance.relPropertyExistenceConstraints.GetEnumerator());
			 }

			 public long nodesWithLabelCardinality( int labelId )
			 {
				  return _outerInstance.nodeCounts.getIfAbsent( labelId, 0L );
			 }

			 public long cardinalityByLabelsAndRelationshipType( int fromLabelId, int relTypeId, int toLabelId )
			 {
				  RelSpecifier specifier = new RelSpecifier( fromLabelId, relTypeId, toLabelId );
				  long? result = _outerInstance.relCounts[specifier];
				  return result == null ? 0L : result.Value;
			 }

			 public double indexUniqueValueSelectivity( int labelId, params int[] propertyKeyIds )
			 {
				  SchemaDescriptor descriptor = SchemaDescriptorFactory.forLabel( labelId, propertyKeyIds );
				  IndexStatistics result1 = _outerInstance.regularIndices.getIndex( descriptor );
				  IndexStatistics result2 = result1 == null ? _outerInstance.uniqueIndices.getIndex( descriptor ) : result1;
				  return result2 == null ? Double.NaN : result2.UniqueValuesPercentage;
			 }

			 public double indexPropertyExistsSelectivity( int labelId, params int[] propertyKeyIds )
			 {
				  SchemaDescriptor descriptor = SchemaDescriptorFactory.forLabel( labelId, propertyKeyIds );
				  IndexStatistics result1 = _outerInstance.regularIndices.getIndex( descriptor );
				  IndexStatistics result2 = result1 == null ? _outerInstance.uniqueIndices.getIndex( descriptor ) : result1;
				  double indexSize = result2 == null ? Double.NaN : result2.Size;
				  return indexSize / nodesWithLabelCardinality( labelId );
			 }

			 private IEnumerator<Pair<string, string[]>> idsToNames<T1>( IEnumerable<T1> nodeConstraints ) where T1 : Neo4Net.Kernel.Api.Internal.schema.LabelSchemaSupplier
			 {
				  return Iterators.map(nodeConstraint =>
				  {
					string label = _outerInstance.labels.byIdOrFail( nodeConstraint.schema().LabelId );
					string[] propertyKeyNames = _outerInstance.propertyKeys.byIdOrFail( nodeConstraint.schema().PropertyIds );
					return Pair.of( label, propertyKeyNames );
				  }, nodeConstraints.GetEnumerator());
			 }
		 }

		 public override void VisitLabel( int labelId, string labelName )
		 {
			  _labels.putToken( labelId, labelName );
		 }

		 public override void VisitPropertyKey( int propertyKeyId, string propertyKeyName )
		 {
			  _propertyKeys.putToken( propertyKeyId, propertyKeyName );
		 }

		 public override void VisitRelationshipType( int relTypeId, string relTypeName )
		 {
			  _relationshipTypes.putToken( relTypeId, relTypeName );
		 }

		 public override void VisitIndex( IndexDescriptor descriptor, string userDescription, double uniqueValuesPercentage, long size )
		 {
			  IndexDescriptorMap indices = descriptor.Type() == UNIQUE ? _uniqueIndices : _regularIndices;
			  indices.PutIndex( descriptor.Schema(), userDescription, uniqueValuesPercentage, size );
		 }

		 public override void VisitUniqueConstraint( UniquenessConstraintDescriptor constraint, string userDescription )
		 {
			  if ( !_uniquenessConstraints.Add( constraint ) )
			  {
					throw new System.ArgumentException( format( "Duplicated unique constraint %s for %s", constraint, userDescription ) );
			  }
		 }

		 public override void VisitNodePropertyExistenceConstraint( NodeExistenceConstraintDescriptor constraint, string userDescription )
		 {
			  if ( !_nodePropertyExistenceConstraints.Add( constraint ) )
			  {
					throw new System.ArgumentException( format( "Duplicated node property existence constraint %s for %s", constraint, userDescription ) );
			  }
		 }

		 public override void VisitRelationshipPropertyExistenceConstraint( RelExistenceConstraintDescriptor constraint, string userDescription )
		 {
			  if ( !_relPropertyExistenceConstraints.Add( constraint ) )
			  {
					throw new System.ArgumentException( format( "Duplicated relationship property existence constraint %s for %s", constraint, userDescription ) );
			  }
		 }

		 public override void VisitNodeKeyConstraint( NodeKeyConstraintDescriptor constraint, string userDescription )
		 {
			  if ( !_nodeKeyConstraints.Add( constraint ) )
			  {
					throw new System.ArgumentException( format( "Duplicated node key constraint %s for %s", constraint, userDescription ) );
			  }
		 }

		 public override void VisitAllNodesCount( long nodeCount )
		 {
			  if ( _allNodesCount < 0 )
			  {
					_allNodesCount = nodeCount;
			  }
			  else
			  {
					throw new System.InvalidOperationException( "Already received node count" );
			  }
		 }

		 public override void VisitNodeCount( int labelId, string labelName, long nodeCount )
		 {
			  if ( _nodeCounts.containsKey( labelId ) )
			  {
					throw new System.ArgumentException( format( "Duplicate node count %s for label with id %s", nodeCount, labelName ) );
			  }
			  _nodeCounts.put( labelId, nodeCount );
		 }

		 public override void VisitRelCount( int startLabelId, int relTypeId, int endLabelId, string relCountQuery, long relCount )
		 {
			  RelSpecifier specifier = new RelSpecifier( startLabelId, relTypeId, endLabelId );

			  if ( _relCounts.put( specifier, relCount ) != null )
			  {
					throw new System.ArgumentException( format( "Duplicate rel count %s for relationship specifier %s (corresponding query: %s)", relCount, specifier, relCountQuery ) );
			  }
		 }

		 private class RelSpecifier
		 {
			  public readonly int FromLabelId;
			  public readonly int RelTypeId;
			  public readonly int ToLabelId;

			  internal RelSpecifier( int fromLabelId, int relTypeId, int toLabelId )
			  {
					this.FromLabelId = fromLabelId;
					this.RelTypeId = relTypeId;
					this.ToLabelId = toLabelId;
			  }

			  public override string ToString()
			  {
					return format( "RelSpecifier{fromLabelId=%d, relTypeId=%d, toLabelId=%d}", FromLabelId, RelTypeId, ToLabelId );
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

					RelSpecifier that = ( RelSpecifier ) o;
					return FromLabelId == that.FromLabelId && RelTypeId == that.RelTypeId && ToLabelId == that.ToLabelId;
			  }

			  public override int GetHashCode()
			  {
					int result = FromLabelId;
					result = 31 * result + RelTypeId;
					result = 31 * result + ToLabelId;
					return result;
			  }
		 }

		 private class IndexStatistics
		 {
			  internal readonly double UniqueValuesPercentage;
			  internal readonly long Size;

			  internal IndexStatistics( double uniqueValuesPercentage, long size )
			  {
					this.UniqueValuesPercentage = uniqueValuesPercentage;
					this.Size = size;
			  }
		 }

		 private class IndexDescriptorMap : IEnumerable<Pair<string[], string[]>>
		 {
			 private readonly DbStructureCollector _outerInstance;

			  internal readonly string IndexType;
			  internal readonly IDictionary<SchemaDescriptor, IndexStatistics> IndexMap = new Dictionary<SchemaDescriptor, IndexStatistics>();

			  internal IndexDescriptorMap( DbStructureCollector outerInstance, string indexType )
			  {
				  this._outerInstance = outerInstance;
					this.IndexType = indexType;
			  }

			  public virtual void PutIndex( SchemaDescriptor descriptor, string userDescription, double uniqueValuesPercentage, long size )
			  {
					if ( IndexMap.ContainsKey( descriptor ) )
					{
						 throw new System.ArgumentException( format( "Duplicate index descriptor %s for %s index %s", descriptor, IndexType, userDescription ) );
					}

					IndexMap[descriptor] = new IndexStatistics( uniqueValuesPercentage, size );
			  }

			  public virtual IndexStatistics GetIndex( SchemaDescriptor descriptor )
			  {
					return IndexMap[descriptor];
			  }

			  public override IEnumerator<Pair<string[], string[]>> Iterator()
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Iterator<org.Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor> iterator = indexMap.keySet().iterator();
					IEnumerator<SchemaDescriptor> iterator = IndexMap.Keys.GetEnumerator();
					return new IteratorAnonymousInnerClass( this, iterator );
			  }

			  private class IteratorAnonymousInnerClass : IEnumerator<Pair<string[], string[]>>
			  {
				  private readonly IndexDescriptorMap _outerInstance;

				  private IEnumerator<SchemaDescriptor> _iterator;

				  public IteratorAnonymousInnerClass( IndexDescriptorMap outerInstance, IEnumerator<SchemaDescriptor> iterator )
				  {
					  this.outerInstance = outerInstance;
					  this._iterator = iterator;
				  }

				  public bool hasNext()
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						return _iterator.hasNext();
				  }

				  public Pair<string[], string[]> next()
				  {
						//TODO: Add support for composite indexes
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						SchemaDescriptor next = _iterator.next();
						EntityType type = next.EntityType();
						string[] IEntityTokens;
						switch ( type.innerEnumValue )
						{
						case EntityType.InnerEnum.NODE:
							 IEntityTokens = _outerInstance._outerInstance.labels.byIdOrFail( next.EntityTokenIds );
							 break;
						case EntityType.InnerEnum.RELATIONSHIP:
							 IEntityTokens = _outerInstance._outerInstance.relationshipTypes.byIdOrFail( next.EntityTokenIds );
							 break;
						default:
							 throw new System.InvalidOperationException( "Indexing is not supported for EntityType: " + type );
						}
						string[] propertyKeyNames = _outerInstance._outerInstance.propertyKeys.byIdOrFail( next.PropertyIds );
						return Pair.of( IEntityTokens, propertyKeyNames );
				  }

				  public void remove()
				  {
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						_iterator.remove();
				  }
			  }
		 }

		 private class TokenMap : IEnumerable<Pair<int, string>>
		 {
			  internal readonly string TokenType;
			  internal readonly IDictionary<int, string> Forward = new Dictionary<int, string>();
			  internal readonly IDictionary<string, int> Backward = new Dictionary<string, int>();

			  internal TokenMap( string tokenType )
			  {
					this.TokenType = tokenType;
			  }

			  public virtual string ByIdOrFail( int token )
			  {
					string result = Forward[token];
					if ( string.ReferenceEquals( result, null ) )
					{
						 throw new System.ArgumentException( format( "Didn't find %s token with id %s", TokenType, token ) );
					}
					return result;
			  }

			  public virtual string[] ByIdOrFail( int[] tokens )
			  {
					string[] results = new string[tokens.Length];
					for ( int i = 0; i < tokens.Length; i++ )
					{
						 results[i] = ByIdOrFail( tokens[i] );
					}
					return results;
			  }

			  public virtual void PutToken( int token, string name )
			  {
					if ( Forward.ContainsKey( token ) )
					{
						 throw new System.ArgumentException( format( "Duplicate id %s for name %s in %s token map", token, name, TokenType ) );
					}

					if ( Backward.ContainsKey( name ) )
					{
						 throw new System.ArgumentException( format( "Duplicate name %s for id %s in %s token map", name, token, TokenType ) );
					}

					Forward[token] = name;
					Backward[name] = token;
			  }

			  public override IEnumerator<Pair<int, string>> Iterator()
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Iterator<java.util.Map.Entry<int, String>> iterator = forward.entrySet().iterator();
					IEnumerator<KeyValuePair<int, string>> iterator = Forward.SetOfKeyValuePairs().GetEnumerator();
					return new IteratorAnonymousInnerClass( this, iterator );
			  }

			  private class IteratorAnonymousInnerClass : IEnumerator<Pair<int, string>>
			  {
				  private readonly TokenMap _outerInstance;

				  private IEnumerator<KeyValuePair<int, string>> _iterator;

				  public IteratorAnonymousInnerClass( TokenMap outerInstance, IEnumerator<KeyValuePair<int, string>> iterator )
				  {
					  this.outerInstance = outerInstance;
					  this._iterator = iterator;
				  }

				  public bool hasNext()
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						return _iterator.hasNext();
				  }

				  public Pair<int, string> next()
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						KeyValuePair<int, string> next = _iterator.next();
						return Pair.of( next.Key, next.Value );
				  }

				  public void remove()
				  {
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						_iterator.remove();
				  }
			  }
		 }
	}

}