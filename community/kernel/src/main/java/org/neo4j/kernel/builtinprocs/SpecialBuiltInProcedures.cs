﻿/*
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
namespace Org.Neo4j.Kernel.builtinprocs
{

	using Org.Neo4j.Function;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.ProcedureSignature.procedureName;

	/// <summary>
	/// This class houses built-in procedures which use a backdoor to inject dependencies.
	/// <para>
	/// TODO: The dependencies should be made available by a standard mechanism so the backdoor is not needed.
	/// </para>
	/// </summary>
	public class SpecialBuiltInProcedures : ThrowingConsumer<Procedures, ProcedureException>
	{
		 private readonly string _neo4jVersion;
		 private readonly string _neo4jEdition;

		 public SpecialBuiltInProcedures( string neo4jVersion, string neo4jEdition )
		 {
			  this._neo4jVersion = neo4jVersion;
			  this._neo4jEdition = neo4jEdition;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void accept(org.neo4j.kernel.impl.proc.Procedures procs) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override void Accept( Procedures procs )
		 {
			  procs.Register( new ListComponentsProcedure( procedureName( "dbms", "components" ), _neo4jVersion, _neo4jEdition ) );
			  procs.Register( new JmxQueryProcedure( procedureName( "dbms", "queryJmx" ), ManagementFactory.PlatformMBeanServer ) );
		 }
	}

}