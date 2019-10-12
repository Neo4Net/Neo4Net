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
namespace Neo4Net.Bolt.v1.messaging
{

	using Neo4jPack = Neo4Net.Bolt.messaging.Neo4jPack;
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using AckFailureMessage = Neo4Net.Bolt.v1.messaging.request.AckFailureMessage;
	using DiscardAllMessage = Neo4Net.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Neo4Net.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using GoodbyeMessage = Neo4Net.Bolt.v3.messaging.request.GoodbyeMessage;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;

	public class BoltRequestMessageWriter
	{
		 protected internal readonly Neo4Net.Bolt.messaging.Neo4jPack_Packer Packer;

		 public BoltRequestMessageWriter( Neo4Net.Bolt.messaging.Neo4jPack_Packer packer )
		 {
			  this.Packer = packer;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public BoltRequestMessageWriter write(org.neo4j.bolt.messaging.RequestMessage message) throws java.io.IOException
		 public virtual BoltRequestMessageWriter Write( RequestMessage message )
		 {
			  if ( message is InitMessage )
			  {
					WriteInit( ( InitMessage ) message );
			  }
			  else if ( message is AckFailureMessage )
			  {
					WriteAckFailure();
			  }
			  else if ( message is ResetMessage )
			  {
					WriteReset();
			  }
			  else if ( message is RunMessage )
			  {
					WriteRun( ( RunMessage ) message );
			  }
			  else if ( message is DiscardAllMessage )
			  {
					WriteDiscardAll();
			  }
			  else if ( message is PullAllMessage )
			  {
					WritePullAll();
			  }
			  else if ( message is GoodbyeMessage )
			  {
					WriteGoodbye();
			  }
			  else
			  {
					throw new System.ArgumentException( "Unknown message: " + message );
			  }
			  return this;
		 }

		 private void WriteGoodbye()
		 {
			  try
			  {
					Packer.packStructHeader( 0, GoodbyeMessage.SIGNATURE );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private void WriteInit( InitMessage message )
		 {
			  try
			  {
					Packer.packStructHeader( 2, InitMessage.SIGNATURE );
					Packer.pack( message.UserAgent() );
					Packer.pack( ValueUtils.asMapValue( message.AuthToken() ) );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private void WriteAckFailure()
		 {
			  try
			  {
					Packer.packStructHeader( 0, AckFailureMessage.SIGNATURE );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private void WriteReset()
		 {
			  try
			  {
					Packer.packStructHeader( 0, ResetMessage.SIGNATURE );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private void WriteRun( RunMessage message )
		 {
			  try
			  {
					Packer.packStructHeader( 2, RunMessage.SIGNATURE );
					Packer.pack( message.Statement() );
					Packer.pack( message.Params() );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private void WriteDiscardAll()
		 {
			  try
			  {
					Packer.packStructHeader( 0, DiscardAllMessage.SIGNATURE );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private void WritePullAll()
		 {
			  try
			  {
					Packer.packStructHeader( 0, PullAllMessage.SIGNATURE );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public virtual void Flush()
		 {
			  try
			  {
					Packer.flush();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }
	}

}