using System.Collections.Generic;
using System.Diagnostics;

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

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.FeatureToggles.flag;

	internal class KeyValueWriter : System.IDisposable
	{
		 private readonly MetadataCollector _metadata;
		 private readonly Writer _writer;
		 private int _keySize;
		 private int _valueSize;
		 private State _state = State.ExpectingFormatSpecifier;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static KeyValueWriter create(MetadataCollector metadata, org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.io.pagecache.PageCache pages, java.io.File path, int pageSize) throws java.io.IOException
		 public static KeyValueWriter Create( MetadataCollector metadata, FileSystemAbstraction fs, PageCache pages, File path, int pageSize )
		 {
			  return new KeyValueWriter( metadata, Writer.Create( fs, pages, path, pageSize ) );
		 }

		 internal KeyValueWriter( MetadataCollector metadata, Writer writer )
		 {
			  this._metadata = metadata;
			  this._writer = writer;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean writeHeader(BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value) throws java.io.IOException
		 public virtual bool WriteHeader( BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value )
		 {
			  bool result = _state.header( this, value.AllZeroes() || value.MinusOneAtTheEnd() );
			  DoWrite( key, value, State.Done );
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeData(BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value) throws java.io.IOException
		 public virtual void WriteData( BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value )
		 {
			  _state.data( this );
			  Debug.Assert( key.Size() == _keySize );
			  Debug.Assert( value.Size() == _valueSize );
			  if ( key.AllZeroes() )
			  {
					_state = State.InError;
					throw new System.ArgumentException( "All-zero keys are not allowed." );
			  }
			  if ( !Write( key, value ) )
			  {
					_state = State.InError;
					throw new System.InvalidOperationException( "MetadataCollector stopped on data field." );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void doWrite(BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value, State expectedNextState) throws java.io.IOException
		 private void DoWrite( BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value, State expectedNextState )
		 {
			  this._keySize = key.Size();
			  this._valueSize = value.Size();
			  Debug.Assert( key.AllZeroes(), "key should have been cleared by previous call" );
			  if ( !Write( key, value ) )
			  {
					if ( _state != expectedNextState )
					{
						 _state = State.InError;
						 throw new System.InvalidOperationException( "MetadataCollector stopped before " + expectedNextState + " reached." );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean write(BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value) throws java.io.IOException
		 private bool Write( BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value )
		 {
			  bool result = _metadata.visit( key, value );
			  _writer.write( key.Buffer );
			  _writer.write( value.Buffer );
			  key.Clear();
			  value.Clear();
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public KeyValueStoreFile openStoreFile() throws java.io.IOException
		 public virtual KeyValueStoreFile OpenStoreFile()
		 {
			  _state.open( this );
			  return _writer.open( _metadata, _keySize, _valueSize );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _writer.close();
		 }

		 private sealed class State
		 { // <pre>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           expecting_format_specifier { boolean header(KeyValueWriter writer, boolean zeroValueOrMinusOne) { if(zeroValueOrMinusOne) { writer.state = in_error; return false; } else { writer.state = expecting_header; return true; } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           expecting_header { boolean header(KeyValueWriter writer, boolean zeroValueOrMinusOne) { writer.state = zeroValueOrMinusOne ? expecting_data : writing_header; return true; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           writing_header { boolean header(KeyValueWriter writer, boolean zeroValueOrMinusOne) { if(zeroValueOrMinusOne) { writer.state = done; } return true; } void data(KeyValueWriter writer) { writer.state = writing_data; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           expecting_data { boolean header(KeyValueWriter writer, boolean zeroValueOrMinusOne) { if(zeroValueOrMinusOne) { writer.state = done; return true; } else { writer.state = in_error; return false; } } void data(KeyValueWriter writer) { writer.state = writing_data; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           writing_data { boolean header(KeyValueWriter writer, boolean zeroValueOrMinusOne) { if(zeroValueOrMinusOne) { writer.state = in_error; return false; } else { writer.state = done; return true; } } void data(KeyValueWriter writer) { } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           done { void open(KeyValueWriter writer) { } },
			  public static readonly State InError = new State( "InError", InnerEnum.InError );

			  private static readonly IList<State> valueList = new List<State>();

			  static State()
			  {
				  valueList.Add( expecting_format_specifier );
				  valueList.Add( expecting_header );
				  valueList.Add( writing_header );
				  valueList.Add( expecting_data );
				  valueList.Add( writing_data );
				  valueList.Add( done );
				  valueList.Add( InError );
			  }

			  public enum InnerEnum
			  {
				  expecting_format_specifier,
				  expecting_header,
				  writing_header,
				  expecting_data,
				  writing_data,
				  done,
				  InError
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private State( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }
			  // </pre>

			  internal bool Header( KeyValueWriter writer, bool zeroValueOrMinusOne )
			  {
					throw IllegalState( writer, "write header" );
			  }

			  internal void Data( KeyValueWriter writer )
			  {
					throw IllegalState( writer, "write data" );
			  }

			  internal void Open( KeyValueWriter writer )
			  {
					throw IllegalState( writer, "open store file" );
			  }

			  internal System.InvalidOperationException IllegalState( KeyValueWriter writer, string what )
			  {
					writer.state = in_error;
					return new System.InvalidOperationException( "Cannot " + what + " when " + name().replace('_', ' ') + "." );
			  }

			 public static IList<State> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static State valueOf( string name )
			 {
				 foreach ( State enumInstance in State.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 internal abstract class Writer
		 {
			  internal static readonly bool WriteToPageCache = flag( typeof( KeyValueWriter ), "WRITE_TO_PAGE_CACHE", false );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void write(byte[] data) throws java.io.IOException;
			  internal abstract void Write( sbyte[] data );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract KeyValueStoreFile open(Metadata metadata, int keySize, int valueSize) throws java.io.IOException;
			  internal abstract KeyValueStoreFile Open( Metadata metadata, int keySize, int valueSize );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void close() throws java.io.IOException;
			  internal abstract void Close();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static Writer create(org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.io.pagecache.PageCache pages, java.io.File path, int pageSize) throws java.io.IOException
			  internal static Writer Create( FileSystemAbstraction fs, PageCache pages, File path, int pageSize )
			  {
					if ( pages == null )
					{
						 return new StreamWriter( fs.OpenAsOutputStream( path, false ) );
					}
					else if ( WriteToPageCache )
					{
						 return new PageWriter( pages.Map( path, pageSize ) );
					}
					else
					{
						 return new OpeningStreamWriter( fs.OpenAsOutputStream( path, false ), pages, path, pageSize );
					}
			  }
		 }

		 private class StreamWriter : Writer
		 {
			  internal readonly Stream Out;

			  internal StreamWriter( Stream @out )
			  {
					this.Out = @out;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void write(byte[] data) throws java.io.IOException
			  internal override void Write( sbyte[] data )
			  {
					Out.Write( data, 0, data.Length );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: KeyValueStoreFile open(Metadata metadata, int keySize, int valueSize) throws java.io.IOException
			  internal override KeyValueStoreFile Open( Metadata metadata, int keySize, int valueSize )
			  {
					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void close() throws java.io.IOException
			  internal override void Close()
			  {
					Out.Flush();
					Out.Close();
			  }
		 }

		 private class OpeningStreamWriter : StreamWriter
		 {
			  internal readonly PageCache Pages;
			  internal readonly File Path;
			  internal readonly int PageSize;

			  internal OpeningStreamWriter( Stream @out, PageCache pages, File path, int pageSize ) : base( @out )
			  {
					this.Pages = pages;
					this.Path = path;
					this.PageSize = pageSize;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: KeyValueStoreFile open(Metadata metadata, int keySize, int valueSize) throws java.io.IOException
			  internal override KeyValueStoreFile Open( Metadata metadata, int keySize, int valueSize )
			  {
					return new KeyValueStoreFile( Pages.map( Path, PageSize ), keySize, valueSize, metadata );
			  }
		 }

		 private class PageWriter : Writer
		 {
			  internal readonly PagedFile File;
			  internal PageCursor Cursor;
			  internal bool Opened;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PageWriter(org.neo4j.io.pagecache.PagedFile file) throws java.io.IOException
			  internal PageWriter( PagedFile file )
			  {
					this.File = file;
					this.Cursor = file.Io( 0, Neo4Net.Io.pagecache.PagedFile_Fields.PfSharedWriteLock );
					Cursor.next();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void write(byte[] data) throws java.io.IOException
			  internal override void Write( sbyte[] data )
			  {
					if ( Cursor.Offset == File.pageSize() )
					{
						 Cursor.next();
					}
					Cursor.putBytes( data );
			  }

			  internal override KeyValueStoreFile Open( Metadata metadata, int keySize, int valueSize )
			  {
					KeyValueStoreFile result = new KeyValueStoreFile( File, keySize, valueSize, metadata );
					Opened = true;
					return result;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void close() throws java.io.IOException
			  internal override void Close()
			  {
					Cursor.close();
					Cursor = null;
					if ( Opened )
					{
						 File.flushAndForce();
					}
					else
					{
						 File.close();
					}
			  }
		 }
	}

}