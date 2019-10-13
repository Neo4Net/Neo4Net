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
namespace Neo4Net.Kernel.info
{
	using Log = Neo4Net.Logging.Log;

	public class JvmChecker
	{
		 public static readonly string IncompatibleJvmWarning = "You are using an unsupported Java runtime. Please" +
					" use Oracle(R) Java(TM) Runtime Environment 8, OpenJDK(TM) 8 or IBM J9.";
		 public static readonly string IncompatibleJvmVersionWarning = "You are using an unsupported version of " +
					"the Java runtime. Please use Oracle(R) Java(TM) Runtime Environment 8, OpenJDK(TM) 8 or IBM J9.";
		 public static readonly string NoSerializationFilterWarning = "The version of the Java runtime you are using " +
					" does not include some important security features. Please use a JRE of version 8u121 or higher.";

		 private readonly Log _log;
		 private readonly JvmMetadataRepository _jvmMetadataRepository;

		 public JvmChecker( Log log, JvmMetadataRepository jvmMetadataRepository )
		 {
			  this._log = log;
			  this._jvmMetadataRepository = jvmMetadataRepository;
		 }

		 public virtual void CheckJvmCompatibilityAndIssueWarning()
		 {
			  string javaVmName = _jvmMetadataRepository.JavaVmName;
			  string javaVersion = _jvmMetadataRepository.JavaVersion;

			  if ( !javaVmName.matches( "(Java HotSpot\\(TM\\)|OpenJDK|IBM) (64-Bit Server|Server|Client|J9) VM" ) )
			  {
					_log.warn( IncompatibleJvmWarning );
			  }
			  else if ( !javaVersion.matches( "^1\\.[8].*" ) )
			  {
					_log.warn( IncompatibleJvmVersionWarning );
			  }

			  if ( !SerializationFilterIsAvailable() )
			  {
					_log.warn( NoSerializationFilterWarning );
			  }
		 }

		 public virtual bool SerializationFilterIsAvailable()
		 {
			  //As part of JEP290 ObjectInputFilter was backported to JDK 8 in version 121, but under a different package.
			  Stream<string> classNames = Stream.of( "sun.misc.ObjectInputFilter", "java.io.ObjectInputFilter" );
			  return classNames.anyMatch(className =>
			  {
				try
				{
					 Type.GetType( className );
				}
				catch ( ClassNotFoundException )
				{
					 return false;
				}
				return true;
			  });

		 }
	}

}