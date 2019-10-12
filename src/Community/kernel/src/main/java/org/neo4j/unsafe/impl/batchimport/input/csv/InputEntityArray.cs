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
namespace Neo4Net.@unsafe.Impl.Batchimport.input.csv
{


	/// <summary>
	/// An array of <seealso cref="InputEntity"/> looking like an <seealso cref="InputEntityVisitor"/> to be able to fit into thinks like <seealso cref="Decorator"/>.
	/// </summary>
	public class InputEntityArray : InputEntityVisitor
	{
		 private readonly InputEntity[] _entities;
		 private int _cursor;

		 public InputEntityArray( int length )
		 {
			  this._entities = new InputEntity[length];
		 }

		 public override void Close()
		 {
		 }

		 public override bool PropertyId( long nextProp )
		 {
			  return CurrentEntity().propertyId(nextProp);
		 }

		 public override bool Property( string key, object value )
		 {
			  return CurrentEntity().property(key, value);
		 }

		 public override bool Property( int propertyKeyId, object value )
		 {
			  return CurrentEntity().property(propertyKeyId, value);
		 }

		 public override bool Id( long id )
		 {
			  return CurrentEntity().id(id);
		 }

		 public override bool Id( object id, Group group )
		 {
			  return CurrentEntity().id(id, group);
		 }

		 public override bool Labels( string[] labels )
		 {
			  return CurrentEntity().labels(labels);
		 }

		 public override bool LabelField( long labelField )
		 {
			  return CurrentEntity().labelField(labelField);
		 }

		 public override bool StartId( long id )
		 {
			  return CurrentEntity().startId(id);
		 }

		 public override bool StartId( object id, Group group )
		 {
			  return CurrentEntity().startId(id, group);
		 }

		 public override bool EndId( long id )
		 {
			  return CurrentEntity().endId(id);
		 }

		 public override bool EndId( object id, Group group )
		 {
			  return CurrentEntity().endId(id, group);
		 }

		 public override bool Type( int type )
		 {
			  return CurrentEntity().type(type);
		 }

		 public override bool Type( string type )
		 {
			  return CurrentEntity().type(type);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void endOfEntity() throws java.io.IOException
		 public override void EndOfEntity()
		 {
			  CurrentEntity().endOfEntity();
			  _cursor++;
		 }

		 private InputEntity CurrentEntity()
		 {
			  if ( _entities[_cursor] == null )
			  {
					_entities[_cursor] = new InputEntity();
			  }
			  return _entities[_cursor];
		 }

		 public virtual InputEntity[] ToArray()
		 {
			  return _cursor == _entities.Length ? _entities : Arrays.copyOf( _entities, _cursor );
		 }
	}

}