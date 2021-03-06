﻿using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Impl.Api.state
{
	using IntIterable = org.eclipse.collections.api.IntIterable;
	using IntSets = org.eclipse.collections.impl.factory.primitive.IntSets;

	using CollectionsFactory = Org.Neo4j.Kernel.impl.util.collection.CollectionsFactory;
	using Org.Neo4j.Storageengine.Api;
	using StorageProperty = Org.Neo4j.Storageengine.Api.StorageProperty;
	using RelationshipState = Org.Neo4j.Storageengine.Api.txstate.RelationshipState;
	using Value = Org.Neo4j.Values.Storable.Value;

	internal class RelationshipStateImpl : PropertyContainerStateImpl, RelationshipState
	{
		 internal const RelationshipState org;

		 private class RelationshipStateAnonymousInnerClass : RelationshipState
		 {
			 public long Id
			 {
				 get
				 {
					  throw new System.NotSupportedException( "id not defined" );
				 }
			 }

			 public bool accept<EX>( RelationshipVisitor<EX> visitor ) where EX : Exception
			 {
				  return false;
			 }

			 public IEnumerator<StorageProperty> addedProperties()
			 {
				  return emptyIterator();
			 }

			 public IEnumerator<StorageProperty> changedProperties()
			 {
				  return emptyIterator();
			 }

			 public IntIterable removedProperties()
			 {
				  return IntSets.immutable.empty();
			 }

			 public IEnumerator<StorageProperty> addedAndChangedProperties()
			 {
				  return emptyIterator();
			 }

			 public bool hasPropertyChanges()
			 {
				  return false;
			 }

			 public bool isPropertyChangedOrRemoved( int propertyKey )
			 {
				  return false;
			 }

			 public Value propertyValue( int propertyKey )
			 {
				  return null;
			 }
		 }

		 private long _startNode = -1;
		 private long _endNode = -1;
		 private int _type = -1;

		 internal RelationshipStateImpl( long id, CollectionsFactory collectionsFactory ) : base( id, collectionsFactory )
		 {
		 }

		 internal virtual void SetMetaData( long startNode, long endNode, int type )
		 {
			  this._startNode = startNode;
			  this._endNode = endNode;
			  this._type = type;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <EX extends Exception> boolean accept(org.neo4j.storageengine.api.RelationshipVisitor<EX> visitor) throws EX
		 public override bool Accept<EX>( RelationshipVisitor<EX> visitor ) where EX : Exception
		 {
			  if ( _type != -1 )
			  {
					visitor.Visit( Id, _type, _startNode, _endNode );
					return true;
			  }
			  return false;
		 }
	}

}