/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Neo4Net.Kernel.ha
{
	using ComException = Neo4Net.com.ComException;
	using RequestContext = Neo4Net.com.RequestContext;
	using Neo4Net.com;
	using TransientTransactionFailureException = Neo4Net.Graphdb.TransientTransactionFailureException;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using TokenCreator = Neo4Net.Kernel.impl.core.TokenCreator;

	public abstract class AbstractTokenCreator : TokenCreator
	{
		public abstract void CreateTokens( string[] names, int[] ids, System.Func<int, bool> indexFilter );
		 private readonly Master _master;
		 private readonly RequestContextFactory _requestContextFactory;

		 protected internal AbstractTokenCreator( Master master, RequestContextFactory requestContextFactory )
		 {
			  this._master = master;
			  this._requestContextFactory = requestContextFactory;
		 }

		 public override int CreateToken( string name )
		 {
			  try
			  {
					  using ( Response<int> response = Create( _master, _requestContextFactory.newRequestContext(), name ) )
					  {
						return response.ResponseConflict();
					  }
			  }
			  catch ( ComException e )
			  {
					throw new TransientTransactionFailureException( "Cannot create identifier for token '" + name + "' on the master " + _master + ". " + "The master is either down, or we have network connectivity problems", e );
			  }
		 }

		 protected internal abstract Response<int> Create( Master master, RequestContext context, string name );
	}

}