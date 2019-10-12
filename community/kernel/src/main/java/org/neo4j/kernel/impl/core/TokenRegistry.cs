using System.Collections.Concurrent;
using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.core
{

	using NamedToken = Org.Neo4j.@internal.Kernel.Api.NamedToken;

	/// <summary>
	/// Token registry provide id -> TOKEN and name -> id mappings.
	/// Name -> id mapping will be updated last since it's used to check if the token already exists.
	/// 
	/// Implementation does not provide any atomicity guarantees. Mapping updates will be visible independently from each
	/// other.
	/// Implementation is not thread safe.
	/// </summary>
	public class TokenRegistry
	{
		 private readonly IDictionary<string, int> _nameToId = new ConcurrentDictionary<string, int>();
		 private readonly IDictionary<int, NamedToken> _idToToken = new ConcurrentDictionary<int, NamedToken>();
		 private readonly string _tokenType;

		 public TokenRegistry( string tokenType )
		 {
			  this._tokenType = tokenType;
		 }

		 public virtual string TokenType
		 {
			 get
			 {
				  return _tokenType;
			 }
		 }

		 public virtual IList<NamedToken> InitialTokens
		 {
			 set
			 {
				  _nameToId.Clear();
				  _idToToken.Clear();
				  PutAll( value );
			 }
		 }

		 private void PutAndEnsureUnique( IDictionary<string, int> nameToId, NamedToken token )
		 {
			  int? previous = nameToId.putIfAbsent( token.Name(), token.Id() );
			  if ( previous != null && previous.Value != token.Id() )
			  {
					// since we optimistically put token into a map before, now we need to remove it.
					_idToToken.Remove( token.Id(), token );
					throw new NonUniqueTokenException( _tokenType, token.Name(), token.Id(), previous.Value );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void putAll(java.util.List<org.neo4j.internal.kernel.api.NamedToken> tokens) throws NonUniqueTokenException
		 public virtual void PutAll( IList<NamedToken> tokens )
		 {
			  IDictionary<string, int> newNameToId = new Dictionary<string, int>();
			  IDictionary<int, NamedToken> newIdToToken = new Dictionary<int, NamedToken>();

			  foreach ( NamedToken token in tokens )
			  {
					newIdToToken[token.Id()] = token;
					PutAndEnsureUnique( newNameToId, token );
			  }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  _idToToken.putAll( newIdToToken );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  _nameToId.putAll( newNameToId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void put(org.neo4j.internal.kernel.api.NamedToken token) throws NonUniqueTokenException
		 public virtual void Put( NamedToken token )
		 {
			  _idToToken[token.Id()] = token;
			  PutAndEnsureUnique( _nameToId, token );
		 }

		 public virtual int? GetId( string name )
		 {
			  return _nameToId[name];
		 }

		 public virtual NamedToken GetToken( int id )
		 {
			  return _idToToken[id];
		 }

		 public virtual IEnumerable<NamedToken> AllTokens()
		 {
			  return unmodifiableCollection( _idToToken.Values );
		 }

		 public virtual int Size()
		 {
			  return _nameToId.Count;
		 }
	}

}