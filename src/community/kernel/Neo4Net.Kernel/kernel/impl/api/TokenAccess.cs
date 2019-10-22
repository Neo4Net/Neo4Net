using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Api
{

	using Label = Neo4Net.GraphDb.Label;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Resource = Neo4Net.GraphDb.Resource;
	using Neo4Net.GraphDb;
	using Neo4Net.Helpers.Collections;
	using NamedToken = Neo4Net.Internal.Kernel.Api.NamedToken;
	using SchemaReadCore = Neo4Net.Internal.Kernel.Api.SchemaReadCore;
	using TokenRead = Neo4Net.Internal.Kernel.Api.TokenRead;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Statement = Neo4Net.Kernel.api.Statement;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.StatementConstants.ANY_LABEL;

	public abstract class TokenAccess<R>
	{
		 public static readonly TokenAccess<RelationshipType> RELATIONSHIP_TYPES = new TokenAccessAnonymousInnerClass();

		 private class TokenAccessAnonymousInnerClass : TokenAccess<RelationshipType>
		 {
			 internal override IEnumerator<NamedToken> tokens( TokenRead read )
			 {
				  return read.RelationshipTypesGetAllTokens();
			 }

			 internal override RelationshipType token( NamedToken token )
			 {
				  return RelationshipType.withName( token.Name() );
			 }

			 internal override bool inUse( KernelTransaction transaction, SchemaReadCore schemaReadCore, int tokenId )
			 {
				  return HasAny( schemaReadCore.IndexesGetForRelationshipType( tokenId ) ) || HasAny( schemaReadCore.ConstraintsGetForRelationshipType( tokenId ) ) || transaction.DataRead().countsForRelationship(ANY_LABEL, tokenId, ANY_LABEL) > 0; // used by data
			 }
		 }
		 public static readonly TokenAccess<Label> LABELS = new TokenAccessAnonymousInnerClass2();

		 private class TokenAccessAnonymousInnerClass2 : TokenAccess<Label>
		 {
			 internal override IEnumerator<NamedToken> tokens( TokenRead read )
			 {
				  return read.LabelsGetAllTokens();
			 }

			 internal override Label token( NamedToken token )
			 {
				  return label( token.Name() );
			 }

			 internal override bool inUse( KernelTransaction transaction, SchemaReadCore schemaReadCore, int tokenId )
			 {
				  return HasAny( schemaReadCore.IndexesGetForLabel( tokenId ) ) || HasAny( schemaReadCore.ConstraintsGetForLabel( tokenId ) ) || transaction.DataRead().countsForNode(tokenId) > 0; // used by data
			 }
		 }

		 public static readonly TokenAccess<string> PROPERTY_KEYS = new TokenAccessAnonymousInnerClass3();

		 private class TokenAccessAnonymousInnerClass3 : TokenAccess<string>
		 {
			 internal override IEnumerator<NamedToken> tokens( TokenRead read )
			 {
				  return read.PropertyKeyGetAllTokens();
			 }

			 internal override string token( NamedToken token )
			 {
				  return token.Name();
			 }

			 internal override bool inUse( KernelTransaction transaction, SchemaReadCore schemaReadCore, int tokenId )
			 {
				  return true; // TODO: add support for telling if a property key is in use or not
			 }
		 }

		 public ResourceIterator<R> InUse( KernelTransaction transaction )
		 {
			  return TokenIterator.InUse( transaction, this );
		 }

		 public ResourceIterator<R> All( KernelTransaction transaction )
		 {
			  return TokenIterator.All( transaction, this );
		 }

		 private static bool HasAny<T1>( IEnumerator<T1> iter )
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( iter.hasNext() )
			  {
					return true;
			  }
			  if ( iter is Resource )
			  {
					( ( Resource ) iter ).close();
			  }
			  return false;
		 }

		 private abstract class TokenIterator<T> : PrefetchingResourceIterator<T>
		 {
			  internal Statement Statement;
			  protected internal readonly TokenAccess<T> Access;
			  protected internal readonly IEnumerator<NamedToken> Tokens;

			  internal TokenIterator( KernelTransaction transaction, TokenAccess<T> access )
			  {
					this.Access = access;
					this.Statement = transaction.AcquireStatement();
					try
					{
						 this.Tokens = access.Tokens( transaction.TokenRead() );
					}
					catch ( Exception e )
					{
						 Close();
						 throw e;
					}
			  }

			  public override void Close()
			  {
					if ( Statement != null )
					{
						 Statement.close();
						 Statement = null;
					}
			  }

			  internal static ResourceIterator<T> InUse<T>( KernelTransaction transaction, TokenAccess<T> access )
			  {
					SchemaReadCore schemaReadCore = transaction.SchemaRead().snapshot();
					return new TokenIteratorAnonymousInnerClass( transaction, access, schemaReadCore );
			  }

			  private class TokenIteratorAnonymousInnerClass : TokenIterator<T>
			  {
				  private KernelTransaction _transaction;
				  private SchemaReadCore _schemaReadCore;

				  public TokenIteratorAnonymousInnerClass( KernelTransaction transaction, Neo4Net.Kernel.Impl.Api.TokenAccess<T> access, SchemaReadCore schemaReadCore ) : base( transaction, access )
				  {
					  this._transaction = transaction;
					  this._schemaReadCore = schemaReadCore;
				  }

				  protected internal override T fetchNextOrNull()
				  {
						while ( outerInstance.tokens.MoveNext() )
						{
							 NamedToken token = outerInstance.tokens.Current;
							 if ( this.access.InUse( _transaction, _schemaReadCore, token.Id() ) )
							 {
								  return this.access.Token( token );
							 }
						}
						outerInstance.close();
						return default( T );
				  }
			  }

			  internal static ResourceIterator<T> All<T>( KernelTransaction transaction, TokenAccess<T> access )
			  {
					return new TokenIteratorAnonymousInnerClass2( transaction, access );
			  }

			  private class TokenIteratorAnonymousInnerClass2 : TokenIterator<T>
			  {
				  private new Neo4Net.Kernel.Impl.Api.TokenAccess<T> _access;

				  public TokenIteratorAnonymousInnerClass2( KernelTransaction transaction, Neo4Net.Kernel.Impl.Api.TokenAccess<T> access ) : base( transaction, access )
				  {
					  this._access = access;
				  }

				  protected internal override T fetchNextOrNull()
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						if ( outerInstance.tokens.hasNext() )
						{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 return _access.token( outerInstance.tokens.next() );
						}
						else
						{
							 outerInstance.close();
							 return default( T );
						}
				  }
			  }
		 }

		 internal abstract IEnumerator<NamedToken> Tokens( TokenRead tokenRead );

		 internal abstract R Token( NamedToken token );

		 internal abstract bool InUse( KernelTransaction transaction, SchemaReadCore schemaReadCore, int tokenId );
	}

}