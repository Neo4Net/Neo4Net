using System;
using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using Future = io.netty.util.concurrent.Future;
	using GenericFutureListener = io.netty.util.concurrent.GenericFutureListener;


	using IOUtils = Neo4Net.Io.IOUtils;

	internal class CloseablesListener : AutoCloseable, GenericFutureListener<Future<Void>>
	{
		 private readonly IList<AutoCloseable> _closeables = new List<AutoCloseable>();

		 internal virtual T Add<T>( T closeable ) where T : AutoCloseable
		 {
			  if ( closeable == null )
			  {
					throw new System.ArgumentException( "closeable cannot be null!" );
			  }
			  _closeables.Add( closeable );
			  return closeable;
		 }

		 public override void Close()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  IOUtils.close( Exception::new, _closeables );
		 }

		 public override void OperationComplete( Future<Void> future )
		 {
			  Close();
		 }
	}

}