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
namespace Org.Neo4j.Kernel.Impl.Newapi
{

	using TransactionFailureException = Org.Neo4j.Graphdb.TransactionFailureException;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using NamedToken = Org.Neo4j.@internal.Kernel.Api.NamedToken;
	using Token = Org.Neo4j.@internal.Kernel.Api.Token;
	using LabelNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.LabelNotFoundKernelException;
	using PropertyKeyIdNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.PropertyKeyIdNotFoundKernelException;
	using IllegalTokenNameException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IllegalTokenNameException;
	using TooManyLabelsException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.TooManyLabelsException;
	using AccessMode = Org.Neo4j.@internal.Kernel.Api.security.AccessMode;
	using RelationshipTypeIdNotFoundKernelException = Org.Neo4j.Kernel.Api.Exceptions.RelationshipTypeIdNotFoundKernelException;
	using KernelTransactionImplementation = Org.Neo4j.Kernel.Impl.Api.KernelTransactionImplementation;
	using TokenHolder = Org.Neo4j.Kernel.impl.core.TokenHolder;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using TokenNotFoundException = Org.Neo4j.Kernel.impl.core.TokenNotFoundException;
	using UnderlyingStorageException = Org.Neo4j.Kernel.impl.store.UnderlyingStorageException;
	using StorageReader = Org.Neo4j.Storageengine.Api.StorageReader;

	public class KernelToken : Token
	{
		 private readonly StorageReader _store;
		 private readonly KernelTransactionImplementation _ktx;
		 private readonly TokenHolders _tokenHolders;

		 public KernelToken( StorageReader store, KernelTransactionImplementation ktx, TokenHolders tokenHolders )
		 {
			  this._store = store;
			  this._ktx = ktx;
			  this._tokenHolders = tokenHolders;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int labelGetOrCreateForName(String labelName) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException, org.neo4j.internal.kernel.api.exceptions.schema.TooManyLabelsException
		 public override int LabelGetOrCreateForName( string labelName )
		 {
			  try
			  {
					return GetOrCreateForName( _tokenHolders.labelTokens(), labelName );
			  }
			  catch ( TransactionFailureException e )
			  {
					// Temporary workaround for the property store based label implementation.
					// Actual implementation should not depend on internal kernel exception messages like this.
					if ( e.InnerException is UnderlyingStorageException && e.InnerException.Message.Equals( "Id capacity exceeded" ) )
					{
						 throw new TooManyLabelsException( e );
					}
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void labelGetOrCreateForNames(String[] labelNames, int[] labelIds) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException, org.neo4j.internal.kernel.api.exceptions.schema.TooManyLabelsException
		 public override void LabelGetOrCreateForNames( string[] labelNames, int[] labelIds )
		 {
			  try
			  {
					GetOrCreateForNames( _tokenHolders.labelTokens(), labelNames, labelIds );
			  }
			  catch ( TransactionFailureException e )
			  {
					// Temporary workaround for the property store based label implementation.
					// Actual implementation should not depend on internal kernel exception messages like this.
					if ( e.InnerException is UnderlyingStorageException && e.InnerException.Message.Equals( "Id capacity exceeded" ) )
					{
						 throw new TooManyLabelsException( e );
					}
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int labelCreateForName(String labelName) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException, org.neo4j.internal.kernel.api.exceptions.schema.TooManyLabelsException
		 public override int LabelCreateForName( string labelName )
		 {
			  _ktx.assertOpen();
			  int id = _store.reserveLabelTokenId();
			  _ktx.txState().labelDoCreateForName(labelName, id);
			  return id;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int relationshipTypeCreateForName(String relationshipTypeName) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException
		 public override int RelationshipTypeCreateForName( string relationshipTypeName )
		 {
			  _ktx.assertOpen();
			  int id = _store.reserveRelationshipTypeTokenId();
			  _ktx.txState().relationshipTypeDoCreateForName(relationshipTypeName, id);
			  return id;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int propertyKeyCreateForName(String propertyKeyName) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException
		 public override int PropertyKeyCreateForName( string propertyKeyName )
		 {
			  _ktx.assertOpen();
			  int id = _store.reservePropertyKeyTokenId();
			  _ktx.txState().propertyKeyDoCreateForName(propertyKeyName, id);
			  return id;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int propertyKeyGetOrCreateForName(String propertyKeyName) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException
		 public override int PropertyKeyGetOrCreateForName( string propertyKeyName )
		 {
			  return GetOrCreateForName( _tokenHolders.propertyKeyTokens(), propertyKeyName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void propertyKeyGetOrCreateForNames(String[] propertyKeys, int[] ids) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException
		 public override void PropertyKeyGetOrCreateForNames( string[] propertyKeys, int[] ids )
		 {
			  GetOrCreateForNames( _tokenHolders.propertyKeyTokens(), propertyKeys, ids );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int relationshipTypeGetOrCreateForName(String relationshipTypeName) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException
		 public override int RelationshipTypeGetOrCreateForName( string relationshipTypeName )
		 {
			  return GetOrCreateForName( _tokenHolders.relationshipTypeTokens(), relationshipTypeName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void relationshipTypeGetOrCreateForNames(String[] relationshipTypes, int[] ids) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException
		 public override void RelationshipTypeGetOrCreateForNames( string[] relationshipTypes, int[] ids )
		 {
			  GetOrCreateForNames( _tokenHolders.relationshipTypeTokens(), relationshipTypes, ids );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String nodeLabelName(int labelId) throws org.neo4j.internal.kernel.api.exceptions.LabelNotFoundKernelException
		 public override string NodeLabelName( int labelId )
		 {
			  _ktx.assertOpen();
			  try
			  {
					return _tokenHolders.labelTokens().getTokenById(labelId).name();
			  }
			  catch ( TokenNotFoundException e )
			  {
					throw new LabelNotFoundKernelException( labelId, e );
			  }
		 }

		 public override int NodeLabel( string name )
		 {
			  _ktx.assertOpen();
			  return _tokenHolders.labelTokens().getIdByName(name);
		 }

		 public override int RelationshipType( string name )
		 {
			  _ktx.assertOpen();
			  return _tokenHolders.relationshipTypeTokens().getIdByName(name);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String relationshipTypeName(int relationshipTypeId) throws org.neo4j.kernel.api.exceptions.RelationshipTypeIdNotFoundKernelException
		 public override string RelationshipTypeName( int relationshipTypeId )
		 {
			  _ktx.assertOpen();
			  try
			  {
					return _tokenHolders.relationshipTypeTokens().getTokenById(relationshipTypeId).name();
			  }
			  catch ( TokenNotFoundException e )
			  {
					throw new RelationshipTypeIdNotFoundKernelException( relationshipTypeId, e );
			  }
		 }

		 public override int PropertyKey( string name )
		 {
			  _ktx.assertOpen();
			  return _tokenHolders.propertyKeyTokens().getIdByName(name);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String propertyKeyName(int propertyKeyId) throws org.neo4j.internal.kernel.api.exceptions.PropertyKeyIdNotFoundKernelException
		 public override string PropertyKeyName( int propertyKeyId )
		 {
			  _ktx.assertOpen();
			  try
			  {
					return _tokenHolders.propertyKeyTokens().getTokenById(propertyKeyId).name();
			  }
			  catch ( TokenNotFoundException e )
			  {
					throw new PropertyKeyIdNotFoundKernelException( propertyKeyId, e );
			  }
		 }

		 public override IEnumerator<NamedToken> LabelsGetAllTokens()
		 {
			  _ktx.assertOpen();
			  return _tokenHolders.labelTokens().AllTokens.GetEnumerator();
		 }

		 public override IEnumerator<NamedToken> PropertyKeyGetAllTokens()
		 {
			  _ktx.assertOpen();
			  AccessMode mode = _ktx.securityContext().mode();
			  return Iterators.stream( _tokenHolders.propertyKeyTokens().AllTokens.GetEnumerator() ).filter(propKey => mode.AllowsPropertyReads(propKey.id())).GetEnumerator();
		 }

		 public override IEnumerator<NamedToken> RelationshipTypesGetAllTokens()
		 {
			  _ktx.assertOpen();
			  return _tokenHolders.relationshipTypeTokens().AllTokens.GetEnumerator();
		 }

		 public override int LabelCount()
		 {
			  _ktx.assertOpen();
			  return _store.labelCount();
		 }

		 public override int PropertyKeyCount()
		 {
			  _ktx.assertOpen();
			  return _store.propertyKeyCount();
		 }

		 public override int RelationshipTypeCount()
		 {
			  _ktx.assertOpen();
			  return _store.relationshipTypeCount();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String checkValidTokenName(String name) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException
		 private string CheckValidTokenName( string name )
		 {
			  if ( string.ReferenceEquals( name, null ) || name.Length == 0 )
			  {
					throw new IllegalTokenNameException( name );
			  }
			  return name;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int getOrCreateForName(org.neo4j.kernel.impl.core.TokenHolder tokens, String name) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException
		 private int GetOrCreateForName( TokenHolder tokens, string name )
		 {
			  _ktx.assertOpen();
			  int id = tokens.GetIdByName( CheckValidTokenName( name ) );
			  if ( id != NO_TOKEN )
			  {
					return id;
			  }
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  _ktx.assertAllows( AccessMode::allowsTokenCreates, "Token create" );
			  return tokens.GetOrCreateId( name );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void getOrCreateForNames(org.neo4j.kernel.impl.core.TokenHolder tokenHolder, String[] names, int[] ids) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException
		 private void GetOrCreateForNames( TokenHolder tokenHolder, string[] names, int[] ids )
		 {
			  _ktx.assertOpen();
			  AssertSameLength( names, ids );
			  for ( int i = 0; i < names.Length; i++ )
			  {
					ids[i] = tokenHolder.GetIdByName( CheckValidTokenName( names[i] ) );
					if ( ids[i] == NO_TOKEN )
					{
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
						 _ktx.assertAllows( AccessMode::allowsTokenCreates, "Token create" );
						 tokenHolder.GetOrCreateIds( names, ids );
						 return;
					}
			  }
		 }

		 private void AssertSameLength( string[] names, int[] ids )
		 {
			  if ( names.Length != ids.Length )
			  {
					throw new System.ArgumentException( "Name and id arrays have different length." );
			  }
		 }
	}

}