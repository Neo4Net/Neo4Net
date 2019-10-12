/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Neo4Net.Pushtocloud
{

	using AdminCommand = Neo4Net.Commandline.Admin.AdminCommand;
	using AdminCommandSection = Neo4Net.Commandline.Admin.AdminCommandSection;
	using OutsideWorld = Neo4Net.Commandline.Admin.OutsideWorld;
	using Arguments = Neo4Net.Commandline.Args.Arguments;

	public class PushToCloudCommandProvider : Neo4Net.Commandline.Admin.AdminCommand_Provider
	{
		 public PushToCloudCommandProvider() : base("push-to-cloud")
		 {
		 }

		 public override Arguments AllArguments()
		 {
			  return PushToCloudCommand.Arguments;
		 }

		 public override string Summary()
		 {
			  return "Push database to Neo4j cloud";
		 }

		 public override AdminCommandSection CommandSection()
		 {
			  return AdminCommandSection.general();

		 }

		 public override string Description()
		 {
			  return "Push database to Neo4j cloud. The database can either be a running database or a snapshot in the form of a dump or backup. " +
						 "Target location is... well, some neo4j cloud service somewhere, right?";
		 }

		 public override AdminCommand Create( Path homeDir, Path configDir, OutsideWorld outsideWorld )
		 {
			  return new PushToCloudCommand( homeDir, configDir, outsideWorld, new HttpCopier( outsideWorld ), new RealDumpCreator( homeDir, configDir, outsideWorld ) );
		 }
	}

}