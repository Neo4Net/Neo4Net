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
namespace Neo4Net.Server.rest.repr
{

	public abstract class ScoredEntityRepresentation<E> : ObjectRepresentation, ExtensibleRepresentation, EntityRepresentation where E : ObjectRepresentation, ExtensibleRepresentation, EntityRepresentation
	{
		 private readonly E @delegate;
		 private readonly float _score;

		 protected internal ScoredEntityRepresentation( E scoredObject, float score ) : base( scoredObject.type )
		 {
			  this.@delegate = scoredObject;
			  this._score = score;
		 }

		 protected internal virtual E Delegate
		 {
			 get
			 {
				  return @delegate;
			 }
		 }

		 public virtual string Identity
		 {
			 get
			 {
				  return Delegate.Identity;
			 }
		 }

		 [Mapping("self")]
		 public override ValueRepresentation SelfUri()
		 {
			  return @delegate.selfUri();
		 }

		 [Mapping("score")]
		 public virtual ValueRepresentation Score()
		 {
			  return ValueRepresentation.number( _score );
		 }

		 internal override void ExtraData( MappingSerializer serializer )
		 {
			  @delegate.extraData( serializer );
		 }
	}

}