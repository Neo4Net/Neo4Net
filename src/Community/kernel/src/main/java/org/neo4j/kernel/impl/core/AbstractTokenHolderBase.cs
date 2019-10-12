using System;
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
namespace Neo4Net.Kernel.impl.core
{

	using TransactionFailureException = Neo4Net.Graphdb.TransactionFailureException;
	using NamedToken = Neo4Net.@internal.Kernel.Api.NamedToken;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using ReadOnlyDbException = Neo4Net.Kernel.Api.Exceptions.ReadOnlyDbException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.ALWAYS_FALSE_INT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.TokenRead_Fields.NO_TOKEN;

	public abstract class AbstractTokenHolderBase : TokenHolder
	{
		public abstract void GetOrCreateIds( string[] names, int[] ids );
		 protected internal readonly TokenRegistry TokenRegistry;

		 public AbstractTokenHolderBase( TokenRegistry tokenRegistry )
		 {
			  this.TokenRegistry = tokenRegistry;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setInitialTokens(java.util.List<org.neo4j.internal.kernel.api.NamedToken> tokens) throws NonUniqueTokenException
		 public virtual IList<NamedToken> InitialTokens
		 {
			 set
			 {
				  TokenRegistry.InitialTokens = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void addToken(org.neo4j.internal.kernel.api.NamedToken token) throws NonUniqueTokenException
		 public override void AddToken( NamedToken token )
		 {
			  TokenRegistry.put( token );
		 }

		 public override int GetOrCreateId( string name )
		 {
			  int? id = TokenRegistry.getId( name );
			  if ( id != null )
			  {
					return id.Value;
			  }

			  // Let's create it
			  try
			  {
					return CreateToken( name );
			  }
			  catch ( ReadOnlyDbException e )
			  {
					throw new TransactionFailureException( e.Message, e );
			  }
			  catch ( Exception e )
			  {
					throw new TransactionFailureException( "Could not create token.", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.NamedToken getTokenById(int id) throws TokenNotFoundException
		 public override NamedToken GetTokenById( int id )
		 {
			  NamedToken result = TokenRegistry.getToken( id );
			  if ( result == null )
			  {
					throw new TokenNotFoundException( "Token for id " + id );
			  }
			  return result;
		 }

		 public override int GetIdByName( string name )
		 {
			  int? id = TokenRegistry.getId( name );
			  if ( id == null )
			  {
					return NO_TOKEN;
			  }
			  return id.Value;
		 }

		 public override bool GetIdsByNames( string[] names, int[] ids )
		 {
			  return ResolveIds( names, ids, ALWAYS_FALSE_INT );
		 }

		 public virtual IEnumerable<NamedToken> AllTokens
		 {
			 get
			 {
				  return TokenRegistry.allTokens();
			 }
		 }

		 public override int Size()
		 {
			  return TokenRegistry.size();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract int createToken(String tokenName) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
		 protected internal abstract int CreateToken( string tokenName );

		 internal virtual bool ResolveIds( string[] names, int[] ids, System.Func<int, bool> unresolvedIndexCheck )
		 {
			  bool foundUnresolvable = false;
			  for ( int i = 0; i < ids.Length; i++ )
			  {
					int? id = TokenRegistry.getId( names[i] );
					if ( id != null )
					{
						 ids[i] = id.Value;
					}
					else
					{
						 foundUnresolvable = true;
						 if ( unresolvedIndexCheck( i ) )
						 {
							  // If the check returns `true`, it's a signal that we should stop early.
							  break;
						 }
					}
			  }
			  return foundUnresolvable;
		 }
	}

}