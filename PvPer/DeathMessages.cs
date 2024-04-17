namespace PvPer
{
    public static class DeathMessages
    {
        private static Random Random = new Random();
        public static string[] Messages = new string[] {
            "@0 thought they could out-dance @1. Turns out, they danced themselves to death!",
            "A wild @1 appeared! @0 fainted.",
            "@1 sent @0 flying... literally! Oops.",
            "Looks like @0 forgot to duck. @1 sends their regards!",
            "In the battle of wits, @0 was unarmed against @1.",
            "@1 cast 'Abracadabra' and poof! @0 disappeared.",
            "Roses are red, violets are blue, @1 just defeated @0, woohoo!",
            "@0 thought they could swim in lava. Spoiler alert: They couldn't. @1 is having a blast!",
            "Seems like @1's sword was hungrier than @0 anticipated.",
            "They say curiosity killed the cat. In this case, it was @0, courtesy of @1.",
            "Looks like @0 underestimated the power of gravity. Thanks, @1!",
            "They say revenge is a dish best served cold. Well, @1 served it piping hot to @0!",
            "@0 thought they could outsmart @1. Let's just say it didn't end well.",
            "In the game of PvP, you win or you respawn. Sorry, @0, looks like it's respawn time.",
            "@0 challenged @1 to a duel. Spoiler alert: It didn't go well for @0.",
            "Oops! @0 stepped into @1's trap... literally.",
            "Rumor has it @0 is still searching for their dignity after that defeat by @1.",
            "@1: 1, @0: 0. Better luck next time, @0!",
            "They say practice makes perfect. Looks like @0 needs more practice against @1!",
            "In the clash of titans, @1 emerged victorious over @0. Better luck in the next round, @0!",
            "Looks like @0 got caught in @1's web of destruction. Talk about a sticky situation!",
            "You know it's bad when even the respawn point feels sorry for @0 after that defeat by @1.",
            "Well, @0 tried to play with fire, and @1 brought the extinguisher.",
            "After facing @1, @0 realized that discretion is the better part of valor.",
            "R.I.P. @0's hopes and dreams, courtesy of @1.",
            "Looks like @1 just unlocked the achievement: 'Defeating @0 with Style'.",
            "If at first you don't succeed, respawn and try again, @0!",
            "They say pride comes before the fall. @0 just took a nosedive, thanks to @1.",
            "Whoops! Looks like @0 forgot to read the fine print on @1's invitation to a duel.",
            "In the battle of wits, @1 came armed with a bazooka while @0 brought a toothpick.",
            "@1 just proved that you don't bring a knife to a gunfight, right @0?",
            "And @0 thought they were the cat with nine lives. Turns out, @1's got them beat.",
            "In the clash of legends, @1 left @0 in the dust. Better luck next respawn, @0!",
            "Newsflash: @1 just made @0 rethink their career choice from 'warrior' to 'target'.",
            "Looks like @0's guardian angel called in sick when @1 came around.",
            "Well, @0, at least you can say you gave @1 a good laugh... right before they defeated you.",
            "They say fortune favors the bold. Unfortunately for @0, it favored @1 today.",
            "In the annals of PvP history, this match will forever be remembered as '@0 vs @1: The Great Embarrassment'.",
            "Whoops! Looks like @0 didn't read the warning label on @1's weapon of mass destruction.",
            "Hey @0, here's a tip: next time, duck faster when @1's aiming!",
            "And just like that, @1 turned @0 into pixel dust.",
            "Looks like @0's HP dropped faster than a lead balloon when @1 entered the fray.",
            "Remember when @0 said they were unbeatable? Well, @1 just rewrote that script.",
            "Note to self: Never challenge @1 to a PvP match when they've had their morning coffee.",
            "In the battle of wits, @1 brought a chessboard while @0 brought a checkers set.",
            "After facing @1, @0's respawn button has never looked more inviting.",
            "Breaking news: @1 just broke @0's spirit... and their avatar too.",
            "Looks like @0 got caught in the crosshairs of @1's laser focus.",
            "Well, @0, you might want to consider switching your PvP strategy from 'charge blindly' to 'strategically retreat' after that defeat by @1.",
            "They say practice makes perfect. So, @0, how's that practice against @1 going?"
        };

        public static string GetMessage(string killer, string victim)
        {
            return Messages[Random.Next(Messages.Length)].Replace("@0", victim).Replace("@1", killer);
        }
    }
}