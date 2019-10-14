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
namespace Neo4Net.Index.@internal.gbptree
{
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

	public class SimpleLongLayout : TestLayout<MutableLong, MutableLong>
	{
		 private readonly int _keyPadding;
		 private string _customNameAsMetaData;
		 private readonly bool _fixedSize;
		 private readonly int _identifier;
		 private readonly int _majorVersion;
		 private readonly int _minorVersion;

		 public class Builder
		 {
			  internal int KeyPadding;
			  internal int Identifier = 999;
			  internal int MajorVersion;
			  internal int MinorVersion;
			  internal string CustomNameAsMetaData = "test";
			  internal bool FixedSize = true;

			  public virtual Builder WithKeyPadding( int keyPadding )
			  {
					this.KeyPadding = keyPadding;
					return this;
			  }

			  public virtual Builder WithIdentifier( int identifier )
			  {
					this.Identifier = identifier;
					return this;
			  }

			  public virtual Builder WithMajorVersion( int majorVersion )
			  {
					this.MajorVersion = majorVersion;
					return this;
			  }

			  public virtual Builder WithMinorVersion( int minorVersion )
			  {
					this.MinorVersion = minorVersion;
					return this;
			  }

			  public virtual Builder WithCustomerNameAsMetaData( string customNameAsMetaData )
			  {
					this.CustomNameAsMetaData = customNameAsMetaData;
					return this;
			  }

			  public virtual Builder WithFixedSize( bool fixedSize )
			  {
					this.FixedSize = fixedSize;
					return this;
			  }

			  public virtual SimpleLongLayout Build()
			  {
					return new SimpleLongLayout( KeyPadding, CustomNameAsMetaData, FixedSize, Identifier, MajorVersion, MinorVersion );
			  }
		 }

		 public static Builder LongLayout()
		 {
			  return new Builder();
		 }

		 public SimpleLongLayout( int keyPadding, string customNameAsMetaData, bool fixedSize, int identifier, int majorVersion, int minorVersion )
		 {
			  this._keyPadding = keyPadding;
			  this._customNameAsMetaData = customNameAsMetaData;
			  this._fixedSize = fixedSize;
			  this._identifier = identifier;
			  this._majorVersion = majorVersion;
			  this._minorVersion = minorVersion;
		 }

		 public override int Compare( MutableLong o1, MutableLong o2 )
		 {
			  return Long.compare( o1.longValue(), o2.longValue() );
		 }

		 internal override int CompareValue( MutableLong v1, MutableLong v2 )
		 {
			  return Compare( v1, v2 );
		 }

		 public override MutableLong NewKey()
		 {
			  return new MutableLong();
		 }

		 public override MutableLong CopyKey( MutableLong key, MutableLong into )
		 {
			  into.Value = key.longValue();
			  return into;
		 }

		 public override MutableLong NewValue()
		 {
			  return new MutableLong();
		 }

		 public override int KeySize( MutableLong key )
		 {
			  // pad the key here to affect the max key count, useful to get odd or even max key count
			  return Long.BYTES + _keyPadding;
		 }

		 public override int ValueSize( MutableLong value )
		 {
			  return Long.BYTES;
		 }

		 public override void WriteKey( PageCursor cursor, MutableLong key )
		 {
			  cursor.PutLong( key.longValue() );
			  cursor.PutBytes( _keyPadding, ( sbyte ) 0 );
		 }

		 public override void WriteValue( PageCursor cursor, MutableLong value )
		 {
			  cursor.PutLong( value.longValue() );
		 }

		 public override void ReadKey( PageCursor cursor, MutableLong into, int keySize )
		 {
			  into.Value = cursor.Long;
			  cursor.GetBytes( new sbyte[_keyPadding] );
		 }

		 public override void ReadValue( PageCursor cursor, MutableLong into, int valueSize )
		 {
			  into.Value = cursor.Long;
		 }

		 public override bool FixedSize()
		 {
			  return _fixedSize;
		 }

		 public override long Identifier()
		 {
			  return _identifier;
		 }

		 public override int MajorVersion()
		 {
			  return _majorVersion;
		 }

		 public override int MinorVersion()
		 {
			  return _minorVersion;
		 }

		 public override void WriteMetaData( PageCursor cursor )
		 {
			  WriteString( cursor, _customNameAsMetaData );
			  cursor.PutInt( _keyPadding );
		 }

		 private static void WriteString( PageCursor cursor, string @string )
		 {
			  sbyte[] bytes = @string.GetBytes( UTF_8 );
			  cursor.PutInt( @string.Length );
			  cursor.PutBytes( bytes );
		 }

		 public override void ReadMetaData( PageCursor cursor )
		 {
			  string name = ReadString( cursor );
			  if ( string.ReferenceEquals( name, null ) )
			  {
					return;
			  }

			  if ( !string.ReferenceEquals( _customNameAsMetaData, null ) )
			  {
					if ( !name.Equals( _customNameAsMetaData ) )
					{
						 cursor.CursorException = "Name '" + name + "' doesn't match expected '" + _customNameAsMetaData + "'";
						 return;
					}
			  }
			  _customNameAsMetaData = name;

			  int readKeyPadding = cursor.Int;
			  if ( readKeyPadding != _keyPadding )
			  {
					cursor.CursorException = "Key padding " + readKeyPadding + " doesn't match expected " + _keyPadding;
			  }
		 }

		 private static string ReadString( PageCursor cursor )
		 {
			  int length = cursor.Int;
			  if ( length < 0 || length >= cursor.CurrentPageSize )
			  {
					cursor.CursorException = "Unexpected length of string " + length;
					return null;
			  }

			  sbyte[] bytes = new sbyte[length];
			  cursor.GetBytes( bytes );
			  return StringHelper.NewString( bytes, UTF_8 );
		 }

		 public override MutableLong Key( long seed )
		 {
			  MutableLong key = NewKey();
			  key.Value = seed;
			  return key;
		 }

		 public override MutableLong Value( long seed )
		 {
			  MutableLong value = NewValue();
			  value.Value = seed;
			  return value;
		 }

		 public override long KeySeed( MutableLong key )
		 {
			  return key.Value;
		 }

		 public override long ValueSeed( MutableLong value )
		 {
			  return value.Value;
		 }
	}

}