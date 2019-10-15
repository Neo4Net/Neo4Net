﻿using System;

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
namespace Neo4Net.Server.web
{
	using Logger = org.eclipse.jetty.util.log.Logger;
	using Slf4jLog = org.eclipse.jetty.util.log.Slf4jLog;

	/// <summary>
	/// Slf4jLog.isDebugEnabled delegates in the end to Logback, and since this method is called a lot and that method
	/// is relatively slow, it has a big impact on the overall performance. This subclass fixes that by calling
	/// isDebugEnabled
	/// on creation, and then caches that.
	/// </summary>
	public class FastSlf4jLog : Slf4jLog
	{
		 private bool _debugEnabled;

		 public FastSlf4jLog() : this("org.eclipse.jetty.util.log")
		 {
		 }

		 public FastSlf4jLog( string name ) : base( name )
		 {

			  _debugEnabled = base.DebugEnabled;
		 }

		 public override bool DebugEnabled
		 {
			 get
			 {
				  return _debugEnabled;
			 }
		 }

		 public override void Debug( string msg, params object[] args )
		 {
			  if ( _debugEnabled )
			  {
					if ( args != null && args.Length == 0 )
					{
						 args = null;
					}

					base.Debug( msg, args );
			  }
		 }

		 public override void Debug( Exception thrown )
		 {
			  if ( _debugEnabled )
			  {
					base.Debug( thrown );
			  }
		 }

		 public override void Debug( string msg, Exception thrown )
		 {
			  if ( _debugEnabled )
			  {
					base.Debug( msg, thrown );
			  }
		 }

		 protected internal override Logger NewLogger( string fullname )
		 {
			  return new FastSlf4jLog( fullname );
		 }
	}

}