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
namespace Neo4Net.causalclustering.helper
{

	using Neo4Net.Functions;

	public class CompositeSuspendable : Suspendable
	{
		 private readonly IList<Suspendable> _suspendables = new List<Suspendable>();

		 public virtual void Add( Suspendable suspendable )
		 {
			  _suspendables.Add( suspendable );
		 }

		 public override void Enable()
		 {
			  DoOperation( Suspendable.enable, "Enable" );
		 }

		 public override void Disable()
		 {
			  DoOperation( Suspendable.disable, "Disable" );
		 }

		 private void DoOperation( ThrowingConsumer<Suspendable, Exception> operation, string description )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  ErrorHandler.RunAll( description, _suspendables.Select( ( System.Func<Suspendable, ErrorHandler.ThrowingRunnable> ) suspendable => () => operation.accept(suspendable) ).ToArray(ErrorHandler.ThrowingRunnable[]::new) );
		 }
	}

}