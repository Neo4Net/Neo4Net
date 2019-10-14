/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.state.machines.token
{

	using CoreReplicatedContent = Neo4Net.causalclustering.core.state.machines.tx.CoreReplicatedContent;
	using ReplicatedContentHandler = Neo4Net.causalclustering.messaging.marshalling.ReplicatedContentHandler;

	public class ReplicatedTokenRequest : CoreReplicatedContent
	{
		 private readonly TokenType _type;
		 private readonly string _tokenName;
		 private readonly sbyte[] _commandBytes;

		 public ReplicatedTokenRequest( TokenType type, string tokenName, sbyte[] commandBytes )
		 {
			  this._type = type;
			  this._tokenName = tokenName;
			  this._commandBytes = commandBytes;
		 }

		 public virtual TokenType Type()
		 {
			  return _type;
		 }

		 internal virtual string TokenName()
		 {
			  return _tokenName;
		 }

		 internal virtual sbyte[] CommandBytes()
		 {
			  return _commandBytes;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  ReplicatedTokenRequest that = ( ReplicatedTokenRequest ) o;

			  if ( _type != that._type )
			  {
					return false;
			  }
			  if ( !_tokenName.Equals( that._tokenName ) )
			  {
					return false;
			  }
			  return Arrays.Equals( _commandBytes, that._commandBytes );

		 }

		 public override int GetHashCode()
		 {
			  int result = _type.GetHashCode();
			  result = 31 * result + _tokenName.GetHashCode();
			  result = 31 * result + Arrays.GetHashCode( _commandBytes );
			  return result;
		 }

		 public override string ToString()
		 {
			  return string.Format( "ReplicatedTokenRequest{{type='{0}', name='{1}'}}", _type, _tokenName );
		 }

		 public override void Dispatch( CommandDispatcher commandDispatcher, long commandIndex, System.Action<Result> callback )
		 {
			  commandDispatcher.Dispatch( this, commandIndex, callback );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(org.neo4j.causalclustering.messaging.marshalling.ReplicatedContentHandler contentHandler) throws java.io.IOException
		 public override void Handle( ReplicatedContentHandler contentHandler )
		 {
			  contentHandler.Handle( this );
		 }
	}

}