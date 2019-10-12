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
namespace Org.Neo4j.Kernel.impl.store.counts
{
	using Org.Neo4j.Kernel.impl.store.kvstore;
	using ReadableBuffer = Org.Neo4j.Kernel.impl.store.kvstore.ReadableBuffer;
	using WritableBuffer = Org.Neo4j.Kernel.impl.store.kvstore.WritableBuffer;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;

	internal sealed class FileVersion
	{
		 internal const long InitialTxId = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;
		 internal const int INITIAL_MINOR_VERSION = 0;
		 internal readonly long TxId;
		 internal readonly long MinorVersion;
		 internal static readonly HeaderField<FileVersion> FILE_VERSION = new HeaderFieldAnonymousInnerClass();

		 private class HeaderFieldAnonymousInnerClass : HeaderField<FileVersion>
		 {
			 public FileVersion read( ReadableBuffer header )
			 {
				  return new FileVersion( header.GetLong( 0 ), header.GetLong( 8 ) );
			 }

			 public void write( FileVersion the, WritableBuffer header )
			 {
				  header.PutLong( 0, the.TxId );
				  header.PutLong( 8, the.MinorVersion );
			 }

			 public override string ToString()
			 {
				  return "<Transaction ID>";
			 }
		 }

		 internal FileVersion( long txId ) : this( txId, INITIAL_MINOR_VERSION )
		 {
		 }

		 public FileVersion Update( long txId )
		 {
			  return new FileVersion( txId, this.TxId == txId ? MinorVersion + 1 : INITIAL_MINOR_VERSION );
		 }

		 public override string ToString()
		 {
			  return string.Format( "FileVersion[txId={0:D}, minorVersion={1:D}]", TxId, MinorVersion );
		 }

		 internal FileVersion( long txId, long minorVersion )
		 {

			  this.TxId = txId;
			  this.MinorVersion = minorVersion;
		 }
	}

}