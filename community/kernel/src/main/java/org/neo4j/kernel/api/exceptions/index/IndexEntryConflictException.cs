using System;
using System.Diagnostics;
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
namespace Org.Neo4j.Kernel.Api.Exceptions.index
{
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaUtil = Org.Neo4j.@internal.Kernel.Api.schema.SchemaUtil;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using Value = Org.Neo4j.Values.Storable.Value;
	using ValueTuple = Org.Neo4j.Values.Storable.ValueTuple;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.StatementConstants.NO_SUCH_NODE;

	/// <summary>
	/// TODO why isn't this a <seealso cref="KernelException"/>?
	/// </summary>
	public class IndexEntryConflictException : Exception
	{
		 private readonly ValueTuple _propertyValues;
		 private readonly long _addedNodeId;
		 private readonly long _existingNodeId;

		 /// <summary>
		 /// Make IOUtils happy
		 /// </summary>
		 public IndexEntryConflictException( string message, Exception cause ) : base( message, cause )
		 {
			  _propertyValues = null;
			  _addedNodeId = -1;
			  _existingNodeId = -1;
		 }

		 public IndexEntryConflictException( long existingNodeId, long addedNodeId, params Value[] propertyValue ) : this( existingNodeId, addedNodeId, ValueTuple.of( propertyValue ) )
		 {
		 }

		 public IndexEntryConflictException( long existingNodeId, long addedNodeId, ValueTuple propertyValues ) : base( format( "Both node %d and node %d share the property value %s", existingNodeId, addedNodeId, propertyValues ) )
		 {
			  this._existingNodeId = existingNodeId;
			  this._addedNodeId = addedNodeId;
			  this._propertyValues = propertyValues;
		 }

		 /// <summary>
		 /// Use this method in cases where <seealso cref="org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException"/>
		 /// was caught but it should not have been allowed to be thrown in the first place.
		 /// Typically where the index we performed an operation on is not a unique index.
		 /// </summary>
		 public virtual Exception NotAllowed( IndexDescriptor descriptor )
		 {
			  return new System.InvalidOperationException( string.Format( "Index for ({0}) should not require unique values.", descriptor.UserDescription( SchemaUtil.idTokenNameLookup ) ), this );
		 }

		 public virtual string EvidenceMessage( TokenNameLookup tokenNameLookup, SchemaDescriptor schema )
		 {
			  Debug.Assert( Schema.PropertyIds.length == _propertyValues.size() );

			  string labelName = tokenNameLookup.LabelGetName( Schema.keyId() );
			  if ( _addedNodeId == NO_SUCH_NODE )
			  {
					return format( "Node(%d) already exists with label `%s` and %s", _existingNodeId, labelName, PropertyString( tokenNameLookup, Schema.PropertyIds ) );
			  }
			  else
			  {
					return format( "Both Node(%d) and Node(%d) have the label `%s` and %s", _existingNodeId, _addedNodeId, labelName, PropertyString( tokenNameLookup, Schema.PropertyIds ) );
			  }
		 }

		 public virtual ValueTuple PropertyValues
		 {
			 get
			 {
				  return _propertyValues;
			 }
		 }

		 public virtual Value SinglePropertyValue
		 {
			 get
			 {
				  return _propertyValues.OnlyValue;
			 }
		 }

		 public virtual long AddedNodeId
		 {
			 get
			 {
				  return _addedNodeId;
			 }
		 }

		 public virtual long ExistingNodeId
		 {
			 get
			 {
				  return _existingNodeId;
			 }
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

			  IndexEntryConflictException that = ( IndexEntryConflictException ) o;

			  return _addedNodeId == that._addedNodeId && _existingNodeId == that._existingNodeId && !( _propertyValues != null ?!_propertyValues.Equals( that._propertyValues ) : that._propertyValues != null );
		 }

		 public override int GetHashCode()
		 {
			  int result = _propertyValues != null ? _propertyValues.GetHashCode() : 0;
			  result = 31 * result + ( int )( _addedNodeId ^ ( ( long )( ( ulong )_addedNodeId >> 32 ) ) );
			  result = 31 * result + ( int )( _existingNodeId ^ ( ( long )( ( ulong )_existingNodeId >> 32 ) ) );
			  return result;
		 }

		 public override string ToString()
		 {
			  return "IndexEntryConflictException{" +
						 "propertyValues=" + _propertyValues +
						 ", addedNodeId=" + _addedNodeId +
						 ", existingNodeId=" + _existingNodeId +
						 '}';
		 }

		 private string PropertyString( TokenNameLookup tokenNameLookup, int[] propertyIds )
		 {
			  StringBuilder sb = new StringBuilder();
			  string sep = propertyIds.Length > 1 ? "properties " : "property ";
			  for ( int i = 0; i < propertyIds.Length; i++ )
			  {
					sb.Append( sep );
					sep = ", ";
					sb.Append( '`' );
					sb.Append( tokenNameLookup.PropertyKeyGetName( propertyIds[i] ) );
					sb.Append( "` = " );
					sb.Append( _propertyValues.valueAt( i ).prettyPrint() );
			  }
			  return sb.ToString();
		 }
	}

}