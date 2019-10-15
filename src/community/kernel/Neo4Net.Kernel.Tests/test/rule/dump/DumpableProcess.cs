﻿using System;
using System.Threading;

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
namespace Neo4Net.Test.rule.dump
{

	public class DumpableProcess : UnicastRemoteObject
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public DumpableProcess() throws java.rmi.RemoteException
		 public DumpableProcess() : base()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws Exception
		 public static void Main( string[] args )
		 {
			  ( new DumpableProcess() ).TraceableMethod(args[0]);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void traceableMethod(String signal) throws Exception
		 public virtual void TraceableMethod( string signal )
		 {
			 lock ( this )
			 {
				  // The parent process will listen to this signal to know that it's here.
				  Console.WriteLine( signal );
      
				  while ( true )
				  {
						Monitor.Wait( this );
				  }
			 }
		 }
	}

}