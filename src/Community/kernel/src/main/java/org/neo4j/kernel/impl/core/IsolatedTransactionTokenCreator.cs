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

	using Kernel = Neo4Net.@internal.Kernel.Api.Kernel;
	using Transaction = Neo4Net.@internal.Kernel.Api.Transaction;
	using Transaction_Type = Neo4Net.@internal.Kernel.Api.Transaction_Type;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using IllegalTokenNameException = Neo4Net.@internal.Kernel.Api.exceptions.schema.IllegalTokenNameException;
	using TooManyLabelsException = Neo4Net.@internal.Kernel.Api.exceptions.schema.TooManyLabelsException;
	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;

	/// <summary>
	/// Creates a key within its own transaction, such that the command(s) for creating the key
	/// will be alone in a transaction. If there is a running a transaction while calling this
	/// it will be temporarily suspended meanwhile.
	/// </summary>
	internal abstract class IsolatedTransactionTokenCreator : TokenCreator
	{
		 private readonly System.Func<Kernel> _kernelSupplier;

		 internal IsolatedTransactionTokenCreator( System.Func<Kernel> kernelSupplier )
		 {
			  this._kernelSupplier = kernelSupplier;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized int createToken(String name) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public override int CreateToken( string name )
		 {
			 lock ( this )
			 {
				  Kernel kernel = _kernelSupplier.get();
				  using ( Transaction tx = kernel.BeginTransaction( Transaction_Type.@implicit, LoginContext.AUTH_DISABLED ) )
				  {
						int id = CreateKey( tx, name );
						tx.Success();
						return id;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void createTokens(String[] names, int[] ids, System.Func<int, boolean> filter) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public override void CreateTokens( string[] names, int[] ids, System.Func<int, bool> filter )
		 {
			 lock ( this )
			 {
				  Kernel kernel = _kernelSupplier.get();
				  using ( Transaction tx = kernel.BeginTransaction( Transaction_Type.@implicit, LoginContext.AUTH_DISABLED ) )
				  {
						for ( int i = 0; i < ids.Length; i++ )
						{
							 if ( filter( i ) )
							 {
								  ids[i] = CreateKey( tx, names[i] );
							 }
						}
						tx.Success();
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract int createKey(org.neo4j.internal.kernel.api.Transaction transaction, String name) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException, org.neo4j.internal.kernel.api.exceptions.schema.TooManyLabelsException;
		 internal abstract int CreateKey( Transaction transaction, string name );
	}

}