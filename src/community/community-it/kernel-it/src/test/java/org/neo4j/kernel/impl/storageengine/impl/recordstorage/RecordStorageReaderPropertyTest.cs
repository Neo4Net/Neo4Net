using System;

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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using Test = org.junit.Test;

	using StorageNodeCursor = Neo4Net.Kernel.Api.StorageEngine.StorageNodeCursor;
	using StoragePropertyCursor = Neo4Net.Kernel.Api.StorageEngine.StoragePropertyCursor;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	/// <summary>
	/// Test read access to committed properties.
	/// </summary>
	public class RecordStorageReaderPropertyTest : RecordStorageReaderTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetAllNodeProperties()
		 public virtual void ShouldGetAllNodeProperties()
		 {
			  // GIVEN
			  string longString = "AlalalalalongAlalalalalongAlalalalalongAlalalalalongAlalalalalongAlalalalalongAlalalalalongAlalalalalong";
			  object[] properties = new object[] { longString, CreateNew( typeof( string ) ), CreateNew( typeof( long ) ), CreateNew( typeof( int ) ), CreateNew( typeof( sbyte ) ), CreateNew( typeof( short ) ), CreateNew( typeof( bool ) ), CreateNew( typeof( char ) ), CreateNew( typeof( float ) ), CreateNew( typeof( double ) ), Array( 0, typeof( string ) ), Array( 0, typeof( long ) ), Array( 0, typeof( int ) ), Array( 0, typeof( sbyte ) ), Array( 0, typeof( short ) ), Array( 0, typeof( bool ) ), Array( 0, typeof( char ) ), Array( 0, typeof( float ) ), Array( 0, typeof( double ) ), Array( 1, typeof( string ) ), Array( 1, typeof( long ) ), Array( 1, typeof( int ) ), Array( 1, typeof( sbyte ) ), Array( 1, typeof( short ) ), Array( 1, typeof( bool ) ), Array( 1, typeof( char ) ), Array( 1, typeof( float ) ), Array( 1, typeof( double ) ), Array( 256, typeof( string ) ), Array( 256, typeof( long ) ), Array( 256, typeof( int ) ), Array( 256, typeof( sbyte ) ), Array( 256, typeof( short ) ), Array( 256, typeof( bool ) ), Array( 256, typeof( char ) ), Array( 256, typeof( float ) ), Array( 256, typeof( double ) ) };

			  foreach ( object value in properties )
			  {
					// given
					long nodeId = CreateLabeledNode( Db, singletonMap( "prop", value ), Label1 ).Id;

					// when
					using ( StorageNodeCursor node = StorageReader.allocateNodeCursor() )
					{
						 node.Single( nodeId );
						 assertTrue( node.Next() );

						 using ( StoragePropertyCursor props = StorageReader.allocatePropertyCursor() )
						 {
							  props.Init( node.PropertiesReference() );
							  if ( props.Next() )
							  {
									Value propVal = props.PropertyValue();

									//then
									assertTrue( propVal + ".equals(" + value + ")", propVal.Equals( Values.of( value ) ) );
							  }
							  else
							  {
									fail();
							  }
						 }
					}

			  }
		 }

		 private object Array( int length, Type componentType )
		 {
			  object array = Array.CreateInstance( componentType, length );
			  for ( int i = 0; i < length; i++ )
			  {
					( ( Array )array ).SetValue( CreateNew( componentType ), i );
			  }
			  return array;
		 }

		 private object CreateNew( Type type )
		 {
			  if ( type == typeof( int ) )
			  {
					return 666;
			  }
			  if ( type == typeof( long ) )
			  {
					return 17L;
			  }
			  if ( type == typeof( double ) )
			  {
					return 6.28318530717958647692d;
			  }
			  if ( type == typeof( float ) )
			  {
					return 3.14f;
			  }
			  if ( type == typeof( short ) )
			  {
					return ( short ) 8733;
			  }
			  if ( type == typeof( sbyte ) )
			  {
					return ( sbyte ) 123;
			  }
			  if ( type == typeof( bool ) )
			  {
					return false;
			  }
			  if ( type == typeof( char ) )
			  {
					return 'Z';
			  }
			  if ( type == typeof( string ) )
			  {
					return "hello world";
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new System.ArgumentException( type.FullName );
		 }
	}

}