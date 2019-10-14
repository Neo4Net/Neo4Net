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
namespace Neo4Net.Kernel.impl.store.kvstore
{
	internal class StubCollector : MetadataCollector
	{
		 internal StubCollector( int entriesPerPage, params string[] header ) : base( entriesPerPage, HeaderFields( header ), BigEndianByteArrayBuffer.NewBuffer( 0 ) )
		 {
		 }

		 internal override bool VerifyFormatSpecifier( ReadableBuffer value )
		 {
			  return true;
		 }

		 internal static HeaderField<sbyte[]>[] HeaderFields( string[] keys )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") HeaderField<byte[]>[] fields = new HeaderField[keys.length];
			  HeaderField<sbyte[]>[] fields = new HeaderField[keys.Length];
			  for ( int i = 0; i < keys.Length; i++ )
			  {
					fields[i] = HeaderField( keys[i] );
			  }
			  return fields;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static HeaderField<byte[]> headerField(final String key)
		 private static HeaderField<sbyte[]> HeaderField( string key )
		 {
			  return new HeaderFieldAnonymousInnerClass( key );
		 }

		 private class HeaderFieldAnonymousInnerClass : HeaderField<sbyte[]>
		 {
			 private string _key;

			 public HeaderFieldAnonymousInnerClass( string key )
			 {
				 this._key = key;
			 }

			 public sbyte[] read( ReadableBuffer header )
			 {
				  return header.Get( 0, new sbyte[header.Size()] );
			 }

			 public void write( sbyte[] bytes, WritableBuffer header )
			 {
				  header.Put( 0, bytes );
			 }

			 public override string ToString()
			 {
				  return _key;
			 }
		 }
	}

}