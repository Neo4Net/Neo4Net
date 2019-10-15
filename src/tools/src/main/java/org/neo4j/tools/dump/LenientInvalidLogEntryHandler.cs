﻿using System;

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
namespace Neo4Net.tools.dump
{

	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using InvalidLogEntryHandler = Neo4Net.Kernel.impl.transaction.log.entry.InvalidLogEntryHandler;

	/// <summary>
	/// Is less strict with invalid log entries, allowing reader to try and read through bad sections.
	/// Prints problems along the way.
	/// </summary>
	internal class LenientInvalidLogEntryHandler : InvalidLogEntryHandler
	{
		 private readonly PrintStream @out;

		 internal LenientInvalidLogEntryHandler( PrintStream @out )
		 {
			  this.@out = @out;
		 }

		 public override bool HandleInvalidEntry( Exception e, LogPosition position )
		 {
			  @out.println( "Read broken entry with error:" + e + ", will go one byte ahead and try again" );
			  return true;
		 }

		 public override void BytesSkipped( long bytesSkipped )
		 {
			  @out.println( "... skipped " + bytesSkipped + " bytes of indecipherable transaction log data" );
		 }
	}

}