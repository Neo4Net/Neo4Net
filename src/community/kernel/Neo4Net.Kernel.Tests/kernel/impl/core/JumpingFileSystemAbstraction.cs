using System.IO;

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
namespace Neo4Net.Kernel.impl.core
{

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using DelegatingFileSystemAbstraction = Neo4Net.GraphDb.mockfs.DelegatingFileSystemAbstraction;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using StoreFileChannel = Neo4Net.Io.fs.StoreFileChannel;
	using SchemaStore = Neo4Net.Kernel.impl.store.SchemaStore;
	using DynamicRecordFormat = Neo4Net.Kernel.impl.store.format.standard.DynamicRecordFormat;
	using NodeRecordFormat = Neo4Net.Kernel.impl.store.format.standard.NodeRecordFormat;
	using PropertyRecordFormat = Neo4Net.Kernel.impl.store.format.standard.PropertyRecordFormat;
	using RelationshipGroupRecordFormat = Neo4Net.Kernel.impl.store.format.standard.RelationshipGroupRecordFormat;
	using RelationshipRecordFormat = Neo4Net.Kernel.impl.store.format.standard.RelationshipRecordFormat;
	using ChannelInputStream = Neo4Net.Test.impl.ChannelInputStream;
	using ChannelOutputStream = Neo4Net.Test.impl.ChannelOutputStream;

	public class JumpingFileSystemAbstraction : DelegatingFileSystemAbstraction
	{
		 private readonly int _sizePerJump;

		 public JumpingFileSystemAbstraction( int sizePerJump ) : this( new EphemeralFileSystemAbstraction(), sizePerJump )
		 {
		 }

		 private JumpingFileSystemAbstraction( EphemeralFileSystemAbstraction ephemeralFileSystem, int sizePerJump ) : base( ephemeralFileSystem )
		 {
			  this._sizePerJump = sizePerJump;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.io.fs.StoreChannel open(java.io.File fileName, org.Neo4Net.io.fs.OpenMode openMode) throws java.io.IOException
		 public override StoreChannel Open( File fileName, OpenMode openMode )
		 {
			  StoreFileChannel channel = ( StoreFileChannel ) base.Open( fileName, openMode );
			  if ( fileName.Name.Equals( "neostore.nodestore.db" ) || fileName.Name.Equals( "neostore.nodestore.db.labels" ) || fileName.Name.Equals( "neostore.relationshipstore.db" ) || fileName.Name.Equals( "neostore.propertystore.db" ) || fileName.Name.Equals( "neostore.propertystore.db.strings" ) || fileName.Name.Equals( "neostore.propertystore.db.arrays" ) || fileName.Name.Equals( "neostore.relationshipgroupstore.db" ) )
			  {
					return new JumpingFileChannel( this, channel, RecordSizeFor( fileName ) );
			  }
			  return channel;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.OutputStream openAsOutputStream(java.io.File fileName, boolean append) throws java.io.IOException
		 public override Stream OpenAsOutputStream( File fileName, bool append )
		 {
			  return new ChannelOutputStream( Open( fileName, OpenMode.READ_WRITE ), append );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.InputStream openAsInputStream(java.io.File fileName) throws java.io.IOException
		 public override Stream OpenAsInputStream( File fileName )
		 {
			  return new ChannelInputStream( Open( fileName, OpenMode.READ ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.Reader openAsReader(java.io.File fileName, java.nio.charset.Charset charset) throws java.io.IOException
		 public override Reader OpenAsReader( File fileName, Charset charset )
		 {
			  return new StreamReader( OpenAsInputStream( fileName ), charset );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.Writer openAsWriter(java.io.File fileName, java.nio.charset.Charset charset, boolean append) throws java.io.IOException
		 public override Writer OpenAsWriter( File fileName, Charset charset, bool append )
		 {
			  return new StreamWriter( OpenAsOutputStream( fileName, append ), charset );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.io.fs.StoreChannel create(java.io.File fileName) throws java.io.IOException
		 public override StoreChannel Create( File fileName )
		 {
			  return Open( fileName, OpenMode.READ_WRITE );
		 }

		 public static int GetRecordSize( int dataSize )
		 {
			  return dataSize + DynamicRecordFormat.RECORD_HEADER_SIZE;
		 }

		 private int RecordSizeFor( File fileName )
		 {
			  if ( fileName.Name.EndsWith( "nodestore.db" ) )
			  {
					return NodeRecordFormat.RECORD_SIZE;
			  }
			  else if ( fileName.Name.EndsWith( "relationshipstore.db" ) )
			  {
					return RelationshipRecordFormat.RECORD_SIZE;
			  }
			  else if ( fileName.Name.EndsWith( "propertystore.db.strings" ) || fileName.Name.EndsWith( "propertystore.db.arrays" ) )
			  {
					return GetRecordSize( PropertyRecordFormat.DEFAULT_DATA_BLOCK_SIZE );
			  }
			  else if ( fileName.Name.EndsWith( "propertystore.db" ) )
			  {
					return PropertyRecordFormat.RECORD_SIZE;
			  }
			  else if ( fileName.Name.EndsWith( "nodestore.db.labels" ) )
			  {
					return int.Parse( GraphDatabaseSettings.label_block_size.DefaultValue ) + DynamicRecordFormat.RECORD_HEADER_SIZE;
			  }
			  else if ( fileName.Name.EndsWith( "schemastore.db" ) )
			  {
					return GetRecordSize( SchemaStore.BLOCK_SIZE );
			  }
			  else if ( fileName.Name.EndsWith( "relationshipgroupstore.db" ) )
			  {
					return GetRecordSize( RelationshipGroupRecordFormat.RECORD_SIZE );
			  }
			  throw new System.ArgumentException( fileName.Path );
		 }

		 public class JumpingFileChannel : StoreFileChannel
		 {
			 private readonly JumpingFileSystemAbstraction _outerInstance;

			  internal readonly int RecordSize;

			  internal JumpingFileChannel( JumpingFileSystemAbstraction outerInstance, StoreFileChannel actual, int recordSize ) : base( actual )
			  {
				  this._outerInstance = outerInstance;
					this.RecordSize = recordSize;
			  }

			  internal virtual long TranslateIncoming( long position )
			  {
					return TranslateIncoming( position, false );
			  }

			  internal virtual long TranslateIncoming( long position, bool allowFix )
			  {
					long actualRecord = position / RecordSize;
					if ( actualRecord < outerInstance.sizePerJump / 2 )
					{
						 return position;
					}
					else
					{
						 long jumpIndex = ( actualRecord + outerInstance.sizePerJump ) / 0x100000000L;
						 long diff = actualRecord - jumpIndex * 0x100000000L;
						 diff = AssertWithinDiff( diff, allowFix );
						 long offsetRecord = jumpIndex * outerInstance.sizePerJump + diff;
						 return offsetRecord * RecordSize;
					}
			  }

			  internal virtual long TranslateOutgoing( long offsetPosition )
			  {
					long offsetRecord = offsetPosition / RecordSize;
					if ( offsetRecord < outerInstance.sizePerJump / 2 )
					{
						 return offsetPosition;
					}
					else
					{
						 long jumpIndex = ( offsetRecord - outerInstance.sizePerJump / 2 ) / outerInstance.sizePerJump + 1;
						 long diff = ( ( offsetRecord - outerInstance.sizePerJump / 2 ) % outerInstance.sizePerJump ) - outerInstance.sizePerJump / 2;
						 AssertWithinDiff( diff, false );
						 long actualRecord = jumpIndex * 0x100000000L - outerInstance.sizePerJump / 2 + diff;
						 return actualRecord * RecordSize;
					}
			  }

			  internal virtual long AssertWithinDiff( long diff, bool allowFix )
			  {
					if ( diff < -outerInstance.sizePerJump / 2 || diff > outerInstance.sizePerJump / 2 )
					{
						 if ( allowFix )
						 {
							  // This is needed for shutdown() to work, PropertyStore
							  // gives an invalid offset for truncate.
							  if ( diff < -outerInstance.sizePerJump / 2 )
							  {
									return -outerInstance.sizePerJump / 2;
							  }
							  else
							  {
									return outerInstance.sizePerJump / 2;
							  }
						 }
						 throw new System.ArgumentException( "" + diff );
					}
					return diff;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long position() throws java.io.IOException
			  public override long Position()
			  {
					return TranslateOutgoing( base.Position() );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public JumpingFileChannel position(long newPosition) throws java.io.IOException
			  public override JumpingFileChannel Position( long newPosition )
			  {
					base.Position( TranslateIncoming( newPosition ) );
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long size() throws java.io.IOException
			  public override long Size()
			  {
					return TranslateOutgoing( base.Size() );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public JumpingFileChannel truncate(long size) throws java.io.IOException
			  public override JumpingFileChannel Truncate( long size )
			  {
					base.Truncate( TranslateIncoming( size, true ) );
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(ByteBuffer dst, long position) throws java.io.IOException
			  public override int Read( ByteBuffer dst, long position )
			  {
					return base.Read( dst, TranslateIncoming( position ) );
			  }
		 }
	}

}