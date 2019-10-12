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
namespace Org.Neo4j.Bolt.v3.messaging
{

	using Neo4jPack = Org.Neo4j.Bolt.messaging.Neo4jPack;
	using RequestMessage = Org.Neo4j.Bolt.messaging.RequestMessage;
	using BoltRequestMessageWriter = Org.Neo4j.Bolt.v1.messaging.BoltRequestMessageWriter;
	using BeginMessage = Org.Neo4j.Bolt.v3.messaging.request.BeginMessage;
	using CommitMessage = Org.Neo4j.Bolt.v3.messaging.request.CommitMessage;
	using HelloMessage = Org.Neo4j.Bolt.v3.messaging.request.HelloMessage;
	using RollbackMessage = Org.Neo4j.Bolt.v3.messaging.request.RollbackMessage;
	using RunMessage = Org.Neo4j.Bolt.v3.messaging.request.RunMessage;
	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;

	/// <summary>
	/// This writer simulates the client.
	/// </summary>
	public class BoltRequestMessageWriterV3 : BoltRequestMessageWriter
	{
		 public BoltRequestMessageWriterV3( Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer ) : base( packer )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.bolt.v1.messaging.BoltRequestMessageWriter write(org.neo4j.bolt.messaging.RequestMessage message) throws java.io.IOException
		 public override BoltRequestMessageWriter Write( RequestMessage message )
		 {
			  if ( message is HelloMessage )
			  {
					WriteHello( ( HelloMessage ) message );
			  }
			  else if ( message is BeginMessage )
			  {
					WriteBegin( ( BeginMessage ) message );
			  }
			  else if ( message is CommitMessage )
			  {
					WriteCommit();
			  }
			  else if ( message is RollbackMessage )
			  {
					WriteRollback();
			  }
			  else if ( message is RunMessage )
			  {
					WriteRun( ( RunMessage ) message );
			  }
			  else
			  {
					base.Write( message );
			  }
			  return this;
		 }

		 private void WriteRun( RunMessage message )
		 {
			  try
			  {
					Packer.packStructHeader( 0, RunMessage.SIGNATURE );
					Packer.pack( message.Statement() );
					Packer.pack( message.Params() );
					Packer.pack( message.Meta() );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private void WriteRollback()
		 {
			  WriteSignatureOnlyMessage( RollbackMessage.SIGNATURE );
		 }

		 private void WriteCommit()
		 {
			  WriteSignatureOnlyMessage( CommitMessage.SIGNATURE );
		 }

		 private void WriteBegin( BeginMessage message )
		 {
			  try
			  {
					Packer.packStructHeader( 0, BeginMessage.SIGNATURE );
					Packer.pack( message.Meta() );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }

		 }

		 private void WriteSignatureOnlyMessage( sbyte signature )
		 {
			  try
			  {
					Packer.packStructHeader( 0, signature );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private void WriteHello( HelloMessage message )
		 {
			  try
			  {
					Packer.packStructHeader( 0, HelloMessage.SIGNATURE );
					Packer.pack( ValueUtils.asMapValue( message.Meta() ) );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }
	}

}