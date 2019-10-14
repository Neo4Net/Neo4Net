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
namespace Neo4Net.@unsafe.Impl.Batchimport.input
{

	/// <summary>
	/// Receives calls for extracted data from <seealso cref="InputChunk"/>. This callback design allows for specific methods
	/// using primitives and other optimizations, to avoid garbage.
	/// </summary>
	public interface InputEntityVisitor : System.IDisposable
	{
		 bool PropertyId( long nextProp );

		 bool Property( string key, object value );

		 bool Property( int propertyKeyId, object value );

		 // For nodes
		 bool Id( long id );

		 bool Id( object id, Group group );

		 bool Labels( string[] labels );

		 bool LabelField( long labelField );

		 // For relationships
		 bool StartId( long id );

		 bool StartId( object id, Group group );

		 bool EndId( long id );

		 bool EndId( object id, Group group );

		 bool Type( int type );

		 bool Type( string type );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void endOfEntity() throws java.io.IOException;
		 void EndOfEntity();

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 InputEntityVisitor NULL = new Adapter()
	//	 { // empty
	//	 };
	}

	 public class InputEntityVisitor_Adapter : InputEntityVisitor
	 {
		  public override bool Property( string key, object value )
		  {
				return true;
		  }

		  public override bool Property( int propertyKeyId, object value )
		  {
				return true;
		  }

		  public override bool PropertyId( long nextProp )
		  {
				return true;
		  }

		  public override bool Id( long id )
		  {
				return true;
		  }

		  public override bool Id( object id, Group group )
		  {
				return true;
		  }

		  public override bool Labels( string[] labels )
		  {
				return true;
		  }

		  public override bool StartId( long id )
		  {
				return true;
		  }

		  public override bool StartId( object id, Group group )
		  {
				return true;
		  }

		  public override bool EndId( long id )
		  {
				return true;
		  }

		  public override bool EndId( object id, Group group )
		  {
				return true;
		  }

		  public override bool Type( int type )
		  {
				return true;
		  }

		  public override bool Type( string type )
		  {
				return true;
		  }

		  public override bool LabelField( long labelField )
		  {
				return true;
		  }

		  public override void EndOfEntity()
		  {
		  }

		  public override void Close()
		  {
		  }
	 }

	 public class InputEntityVisitor_Delegate : InputEntityVisitor
	 {
		  internal readonly InputEntityVisitor Actual;

		  public InputEntityVisitor_Delegate( InputEntityVisitor actual )
		  {
				this.Actual = actual;
		  }

		  public override bool PropertyId( long nextProp )
		  {
				return Actual.propertyId( nextProp );
		  }

		  public override bool Property( string key, object value )
		  {
				return Actual.property( key, value );
		  }

		  public override bool Property( int propertyKeyId, object value )
		  {
				return Actual.property( propertyKeyId, value );
		  }

		  public override bool Id( long id )
		  {
				return Actual.id( id );
		  }

		  public override bool Id( object id, Group group )
		  {
				return Actual.id( id, group );
		  }

		  public override bool Labels( string[] labels )
		  {
				return Actual.labels( labels );
		  }

		  public override bool LabelField( long labelField )
		  {
				return Actual.labelField( labelField );
		  }

		  public override bool StartId( long id )
		  {
				return Actual.startId( id );
		  }

		  public override bool StartId( object id, Group group )
		  {
				return Actual.startId( id, group );
		  }

		  public override bool EndId( long id )
		  {
				return Actual.endId( id );
		  }

		  public override bool EndId( object id, Group group )
		  {
				return Actual.endId( id, group );
		  }

		  public override bool Type( int type )
		  {
				return Actual.type( type );
		  }

		  public override bool Type( string type )
		  {
				return Actual.type( type );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void endOfEntity() throws java.io.IOException
		  public override void EndOfEntity()
		  {
				Actual.endOfEntity();
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		  public override void Close()
		  {
				Actual.Dispose();
		  }
	 }

}